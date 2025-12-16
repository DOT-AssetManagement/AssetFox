using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.WorkQueue;
using BridgeCareCore.Interfaces;
using BridgeCareCore.Models;
using BridgeCareCore.Services;
using BridgeCareCore.Services.General_Work_Queue;
using BridgeCareCore.Services.SummaryReport.CommittedProjects;
using BridgeCareCore.Services.Treatment;
using Microsoft.Extensions.DependencyInjection;

namespace BridgeCareCore.StartupExtension
{
    public static class SimulationDataExtension
    {
        public static void AddSimulationData(this IServiceCollection services)
        {
            services.AddSingleton<SequentialWorkQueue<WorkQueueMetadata>>();
            services.AddSingleton<FastSequentialworkQueue<WorkQueueMetadata>>();
            services.AddSingleton<HiddenUploadQueue<WorkQueueMetadata>>();
            services.AddHostedService<SequentialWorkBackgroundService>();
            services.AddHostedService<FastSequentialWorkBackgroundService>();
            services.AddHostedService<HiddenUploadQueueBackgroundService>();
            services.AddHostedService<AttributeValueCacheBuildLaunchingService>();

            services.AddScoped<IGeneralWorkQueueService, GeneralWorkQueueService>();
            services.AddScoped<AttributeService>();
            services.AddScoped<IExcelRawDataImportService, ExcelRawDataImportService>();
            services.AddScoped<IExpressionValidationService, ExpressionValidationService>();
            services.AddScoped<IUserCriteriaRepository, UserCriteriaRepository>();
            services.AddScoped<IMaintainableAssetRepository, MaintainableAssetRepository>();
            services.AddScoped<IInvestmentBudgetsService, InvestmentBudgetsService>();
            services.AddScoped<IPerformanceCurvesService, PerformanceCurvesService>();
            services.AddScoped<ITreatmentService, TreatmentService>();
            services.AddScoped<ICommittedProjectService, CommittedProjectService>();
            services.AddScoped<IExcelRawDataLoadService, ExcelRawDataLoadService>();
            services.AddScoped<ExcelTreatmentLoader>();
            services.AddScoped<UnitOfDataPersistenceWork>();
            services.AddScoped<IUnitOfWork>(x => x.GetRequiredService<UnitOfDataPersistenceWork>());

            services.AddScoped<ISimulationRepository, SimulationRepository>();
            services.AddScoped<IDataSourceMappingService, DataSourceMappingService>();
        }
    }
}
