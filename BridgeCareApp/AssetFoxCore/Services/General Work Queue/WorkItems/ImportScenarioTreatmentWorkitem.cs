using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Reporting.Logging;
using AssetFox.Core.WorkQueue;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Models;
using OfficeOpenXml;
using System.Threading;
using System;
using Microsoft.Extensions.DependencyInjection;
using AssetFox.Core.Hubs;
using AssetFox.Core.DTOs.Enums;
using AssetFoxCore.Controllers;
using AssetFox.Core.Hubs.Services;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.General_Work_Queue.WorkItems
{
    public record ImportScenarioTreatmentWorkitem(Guid SimulationId, ExcelPackage ExcelPackage, string UserId, string treratmentName) : IWorkSpecification<WorkQueueMetadata>

    {
        public string WorkId => WorkQueueWorkIdFactory.CreateId(SimulationId, WorkType.ImportScenarioTreatment);

        public DateTime StartTime { get; set; }

        public string WorkDescription => "Import Scenario Treatment";

        public WorkQueueMetadata Metadata =>
            new WorkQueueMetadata() { WorkType = WorkType.ImportScenarioTreatment, DomainType = DomainType.Simulation, DomainId = SimulationId };

        public string WorkName => treratmentName;

        public void DoWork(IServiceProvider serviceProvider, Action<string> updateStatusOnHandle, CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();

            var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            var _treatmentService = scope.ServiceProvider.GetRequiredService<ITreatmentService>();
            var _queueLogger = new FastWorkQueueLogger(_hubService, UserId, updateStatusOnHandle, WorkId);
            var importResult = _treatmentService.ImportScenarioTreatmentsFile(SimulationId, ExcelPackage, cancellationToken, _queueLogger);
            if (importResult.WarningMessage != null && importResult.WarningMessage.Trim() != "")
            {
                _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastWarning, importResult.WarningMessage);
            }
        }

        public void OnFault(IServiceProvider serviceProvider, string errorMessage)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();

            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastError, $"{TreatmentController.TreatmentError}::ImportScenarioTreatmentsFile - Something was wrong with the provided file");
        }

        public void OnCompletion(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastTaskCompleted, $"Successfully imported treatment: {WorkName}");
            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastImportCompletion, new ImportCompletionDTO()
            {
                Id = Metadata.DomainId,
                WorkType = Metadata.WorkType
            });
        }

        public void OnUpdate(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastFastWorkQueueUpdate, WorkId);
        }
    }
}
