using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.WorkQueue;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Models;
using AssetFoxCore.Services;
using AssetFoxCore.Services.General_Work_Queue;
using AssetFoxCore.Services.SummaryReport.CommittedProjects;
using AssetFoxCore.Services.Treatment;
using Microsoft.Extensions.DependencyInjection;

namespace AssetFoxCore.StartupExtension
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
