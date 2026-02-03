using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.WorkQueue;
using Microsoft.SqlServer.Dac.Model;
using System.Threading;
using System;
using Microsoft.Extensions.DependencyInjection;
using AssetFoxCore.Services.Aggregation;
using System.Threading.Channels;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs;
using AssetFoxCore.Models;
using System.Text;
using System.Collections.Generic;
using AssetFox.Core.Common;
using AssetFox.Core.Reporting.Logging;
using AssetFox.Core.DTOs.Enums;
using AssetFox.Core.Hubs.Services;
using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace AssetFoxCore.Services
{
    public record AggregationWorkitem(Guid NetworkId, string UserId, string NetworkName, Guid dataSourceId) : IWorkSpecification<WorkQueueMetadata>

    {
        public string WorkId => WorkQueueWorkIdFactory.CreateId(NetworkId, WorkType.Aggregation);

        public DateTime StartTime { get; set; }

        public string WorkDescription => "Network Aggregation";

        public string WorkName => NetworkName;

        public WorkQueueMetadata Metadata => new WorkQueueMetadata() { DomainType = DomainType.Network, WorkType = WorkType.Aggregation, DomainId = NetworkId };

        public void DoWork(IServiceProvider serviceProvider, Action<string> updateStatusOnHandle, CancellationToken cancellationToken)
        {
            string AggregationError = "Aggregation Error";
            using var scope = serviceProvider.CreateScope(); 
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            var _aggregationService = scope.ServiceProvider.GetRequiredService<IAggregationService>();
            var _log = scope.ServiceProvider.GetRequiredService<ILog>();
            var channel = Channel.CreateUnbounded<AggregationStatusMemo>();
            var state = new AggregationState();
            var _queueLogger = new GeneralWorkQueueLogger(_hubService, UserId, updateStatusOnHandle, WorkId);
            var timer = KeepUserInformedOfState(state);
            var readTask = Task.Run(() => ReadMessages(channel.Reader));
                 
            try
            {              
                _aggregationService.AggregateNetworkData(channel.Writer, NetworkId, state, dataSourceId, cancellationToken).Wait();
            }
            catch (Exception e)
            {
                state.Status = "Aggregation failed";
                _unitOfWork.NetworkRepo.UpsertNetworkRollupDetail(NetworkId, state.Status);
                _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastAssignDataStatus,
                    new NetworkRollupDetailDTO { NetworkId = NetworkId, Status = state.Status }, 0.0);
                _hubService.SendRealTimeErrorMessage(UserId, $"{AggregationError}::AggregateNetworkData - {e.Message}", e);
            }
            finally
            {
                NotifyUserOfState(state);
                timer.Stop();
                timer.Close();
                channel.Writer.Complete();
                //readTask.Dispose();
            }

            void DoublyBroadcastError(string broadcastError)
            {
                
                _log.Error(broadcastError);
                _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastError, broadcastError);
            }

            async Task ReadMessages(ChannelReader<AggregationStatusMemo> reader)
            {
                while (await reader.WaitToReadAsync())
                {
                    while (reader.TryRead(out AggregationStatusMemo status))
                    {
                        var error = status.ErrorMessage;
                        if (error != null)
                        {
                            DoublyBroadcastError(error);
                        }
                    }
                }
            }

            void NotifyUserOfState(AggregationState state)
            {
                try
                {
                    SendCurrentStatusMessage(state);
                }
                catch
                {
                    // ignore the error
                }
            }

            System.Timers.Timer KeepUserInformedOfState(AggregationState state)
            {
                var timer = new System.Timers.Timer(3000);
                timer.Elapsed += delegate
                {
                    SendCurrentStatusMessage(state);
                };
                timer.Start();
                return timer;
            }
            void SendCurrentStatusMessage(AggregationState state)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                var count = state.Count % 10;
                var periods = StringWithPeriods(count + 1);
                var message = $"{state.Status}{periods}";

                _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastAssignDataStatus,
                    new NetworkRollupDetailDTO { NetworkId = state.NetworkId, Status = message }, state.Percentage);

                var queueMessage = state.Status;
                if (state.Percentage > 0)
                    queueMessage = queueMessage + $": {state.Percentage}%";
                _queueLogger.UpdateWorkQueueStatus(queueMessage);
                state.Count++;
            }

            static string StringWithPeriods(int count)
            {
                var builder = new StringBuilder();
                for (int i = 0; i < count; i++)
                {
                    builder.Append('.');
                }
                return builder.ToString();
            }
        }

        public void OnFault(IServiceProvider serviceProvider, string errorMessage)
        {
            string AggregationError = "Aggregation Error";
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();

            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastError, $"{AggregationError}::NetworkAggregateAccess - {errorMessage}");
        }

        public void OnCompletion(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();

            Debug.WriteLine($"Aggregation completed at {DateTime.Now}");
            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastTaskCompleted, $"Network aggregation on {NetworkName} has completed");
        }

        public void OnUpdate(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastWorkQueueUpdate, WorkId);
        }
    }
}

