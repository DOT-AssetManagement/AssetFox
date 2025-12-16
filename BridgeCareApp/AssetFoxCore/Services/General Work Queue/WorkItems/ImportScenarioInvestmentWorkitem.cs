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
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs;
using AssetFox.Core.DTOs.Enums;
using AssetFoxCore.Controllers;

namespace AssetFoxCore.Services.General_Work_Queue.WorkItems
{
    public record ImportScenarioInvestmentWorkitem(Guid SimulationId, ExcelPackage ExcelPackage, UserCriteriaDTO CurrentUserCriteriaFilter, bool OverwriteBudgets, string UserId, string InvestmentName) : IWorkSpecification<WorkQueueMetadata>

    {
        public string WorkId => WorkQueueWorkIdFactory.CreateId(SimulationId, WorkType.ImportScenarioInvestment);

        public DateTime StartTime { get; set; }

        public string WorkDescription => "Import Sceenario Investment";

        public WorkQueueMetadata Metadata =>
            new WorkQueueMetadata() { WorkType = WorkType.ImportScenarioInvestment, DomainType = DomainType.Simulation, DomainId = SimulationId };

        public string WorkName => InvestmentName;

        public void DoWork(IServiceProvider serviceProvider, Action<string> updateStatusOnHandle, CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();

            var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            var _investmentBudgetService = scope.ServiceProvider.GetRequiredService<IInvestmentBudgetsService>();
            var _queueLogger = new FastWorkQueueLogger(_hubService, UserId, updateStatusOnHandle, WorkId);
            var importResult = _investmentBudgetService.ImportScenarioInvestmentBudgetsFile(SimulationId, ExcelPackage, CurrentUserCriteriaFilter, OverwriteBudgets, cancellationToken, _queueLogger);
            if (importResult.WarningMessage != null)
            {
                _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastWarning, importResult.WarningMessage);
            }
        }

        public void OnFault(IServiceProvider serviceProvider, string errorMessage)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();

            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastError, $"{InvestmentController.InvestmentError}::ImportScenarioInvestmentBudgetsExcelFile - Something was wrong with the provided file");
        }

        public void OnCompletion(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastTaskCompleted, $"Successfully imported investment: {WorkName}");
            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastImportCompletion, new ImportCompletionDTO()
            {
                Id = Metadata.DomainId,
                WorkType = Metadata.WorkType,
                AreBudgetsOverWritten = OverwriteBudgets
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
