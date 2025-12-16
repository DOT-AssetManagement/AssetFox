using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.Hubs.Interfaces;
using AssetFoxCore.Models;
using Microsoft.SqlServer.Dac.Model;
using System.Threading;
using System;
using AssetFox.Core.WorkQueue;
using Microsoft.Extensions.DependencyInjection;
using AssetFox.Core.Reporting.Logging;
using AssetFox.Core.DTOs.Enums;
using AssetFox.Core.Hubs;
using AssetFoxCore.Controllers;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.General_Work_Queue.WorkItems
{
    public record DeleteSimulationWorkitem(Guid SimulationId, string UserId, string scenarioName) : IWorkSpecification<WorkQueueMetadata>

    {
        public string WorkId => WorkQueueWorkIdFactory.CreateId(SimulationId, WorkType.DeleteSimulation);

        public DateTime StartTime { get; set; }

        public string WorkDescription => "Delete Simulation";

        public WorkQueueMetadata Metadata =>
            new WorkQueueMetadata() { WorkType = WorkType.DeleteSimulation, DomainType = DomainType.Simulation, DomainId = SimulationId};

        public string WorkName => scenarioName;

        public void DoWork(IServiceProvider serviceProvider, Action<string> updateStatusOnHandle, CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();

            var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            var _queueLogger = new GeneralWorkQueueLogger(_hubService, UserId, updateStatusOnHandle, WorkId);

           _unitOfWork.SimulationRepo.DeleteSimulation(SimulationId, cancellationToken, _queueLogger);
           _unitOfWork.SimulationRepo.DeleteSimulationReports(SimulationId);
        }

        public void OnFault(IServiceProvider serviceProvider, string errorMessage)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();

            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastError, $"{SimulationController.SimulationError}::DeleteSimulation - {errorMessage}");
        }

        public void OnCompletion(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastTaskCompleted, $"The simulation {WorkName} has been successfully deleted");
            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastSimulationDeletionCompletion, Metadata.WorkType);
        }

        public void OnUpdate(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastWorkQueueUpdate, WorkId);
        }
    }
}
