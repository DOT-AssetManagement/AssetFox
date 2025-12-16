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
using AssetFoxCore.Controllers;
using AssetFoxCore.Models;
using Microsoft.Extensions.DependencyInjection;

namespace AssetFoxCore.Services
{
    public record DeleteSimulationOutputWorkItem(DateTime? LowerBoundDate, DateTime UpperBoundDate, string UserId) : IWorkSpecification<WorkQueueMetadata>
    {
        public string WorkId => DomainId.ToString() + WorkType.DeleteSimulationOutput;

        public DateTime StartTime { get; set; }

        public static Guid DomainId = Guid.Parse("84B10C7B-630D-4A3D-9AEC-BE048DCBD442");
        public string WorkDescription => $"Deleting output from  {LowerBoundDate} to {UpperBoundDate}";

        public WorkQueueMetadata Metadata =>
            new WorkQueueMetadata() { WorkType = WorkType.DeleteSimulationOutput, DomainType = DomainType.Simulation, DomainId = DomainId };

        public string WorkName => $"Delete simulation output";

        public void DoWork(IServiceProvider serviceProvider, Action<string> updateStatusOnHandle, CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();

            var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            var _queueLogger = new GeneralWorkQueueLogger(_hubService, UserId, updateStatusOnHandle, WorkId);

            _unitOfWork.SimulationOutputRepo.DeleteScenarioOutputsWithingDaterange(LowerBoundDate, UpperBoundDate, cancellationToken);
        }

        public void OnFault(IServiceProvider serviceProvider, string errorMessage)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();

            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastError, $"{SimulationController.SimulationError}::DeleteSimulationOutput - {errorMessage}");
        }

        public void OnCompletion(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastTaskCompleted, $"simulation output from {LowerBoundDate} to {UpperBoundDate} has been successfully deleted");
            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastSimulationOutputDeletionCompletion, Metadata.WorkType);
        }

        public void OnUpdate(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastWorkQueueUpdate, WorkId);
        }
    }
}
