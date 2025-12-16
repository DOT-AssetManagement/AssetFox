using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.Common;

using AssetFox.Core.Common.PerformanceMeasurement;

using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Enums;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Hubs.Services;
using AssetFox.Core.Reporting.Logging;
using AssetFox.Core.WorkQueue;

using AssetFox.Validation;
using AssetFoxCore.Models;
using Microsoft.Extensions.DependencyInjection;

namespace AssetFoxCore.Services
{
    public record DeleteNetworkWorkitem(Guid NetworkId, string UserId, string NetworkName) : IWorkSpecification<WorkQueueMetadata>
    {
        public string WorkId => WorkQueueWorkIdFactory.CreateId(NetworkId, WorkType.DeleteNetwork);

        public DateTime StartTime { get; set; }

        public string WorkDescription => "Delete Network";

        public WorkQueueMetadata Metadata =>
            new WorkQueueMetadata() { WorkType = WorkType.DeleteNetwork, DomainType = DomainType.Network, DomainId = NetworkId};

        public string WorkName => NetworkName;

        public void DoWork(IServiceProvider serviceProvider, Action<string> updateStatusOnHandle, CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();

            var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            var _queueLogger = new GeneralWorkQueueLogger(_hubService, UserId, updateStatusOnHandle, WorkId);
            _unitOfWork.NetworkRepo.DeleteNetwork(NetworkId, cancellationToken, _queueLogger);
        }

        public void OnFault(IServiceProvider serviceProvider, string errorMessage)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();

            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastError, $"Network Error::DeleteNetwork - {errorMessage}");
        }

        public void OnCompletion(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastTaskCompleted, $"The network {NetworkName} has been successfully deleted");
        }

        public void OnUpdate(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastWorkQueueUpdate, WorkId);
        }
    }
}
