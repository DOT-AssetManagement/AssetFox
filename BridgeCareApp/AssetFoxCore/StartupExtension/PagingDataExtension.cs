using AssetFoxCore.Interfaces;
using AssetFoxCore.Interfaces.DefaultData;
using AssetFoxCore.Services;
using AssetFoxCore.Services.DefaultData;
using AssetFoxCore.Services.Paging;
using Microsoft.Extensions.DependencyInjection;

namespace AssetFoxCore.StartupExtension
{
    public static class PagingDataExtension
    {
        public static void AddPagingData(this IServiceCollection services)
        {
            services.AddScoped<IDeficientConditionGoalPagingService, DeficientConditionGoalPagingService>();
            services.AddScoped<ITargetConditionGoalPagingService, TargetConditionGoalPagingService>();
            services.AddScoped<IBudgetPriortyPagingService, BudgetPriorityPagingService>();
            services.AddScoped<ICalculatedAttributePagingService, CalculatedAttributePagingService>();
            services.AddScoped<ICommittedProjectPagingService, CommittedProjectPagingService>();
            services.AddScoped<IInvestmentPagingService, InvestmentPagingService>();
            services.AddScoped<IPerformanceCurvesPagingService, PerformanceCurvesPagingService>();
            services.AddScoped<IRemainingLifeLimitPagingService, RemainingLifeLimitPagingService>();
            services.AddScoped<ISimulationPagingService, SimulationPagingService>();
            services.AddScoped<ITreatmentPagingService, TreatmentPagingService>();
            services.AddScoped<IWorkQueueService, WorkQueueService>();
            services.AddScoped<ICashFlowPagingService, CashFlowPagingService>();
        }
    }
}
