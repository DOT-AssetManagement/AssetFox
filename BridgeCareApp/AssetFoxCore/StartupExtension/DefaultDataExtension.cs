using AssetFoxCore.Interfaces.DefaultData;
using AssetFoxCore.Services.DefaultData;
using Microsoft.Extensions.DependencyInjection;

namespace AssetFoxCore.StartupExtension
{
    public static class DefaultDataExtension
    {
        public static void AddDefaultData(this IServiceCollection services)
        {
            services.AddScoped<IAnalysisDefaultDataService, AnalysisDefaultDataService>();
            services.AddScoped<IInvestmentDefaultDataService, InvestmentDefaultDataService>();
        }
    }
}
