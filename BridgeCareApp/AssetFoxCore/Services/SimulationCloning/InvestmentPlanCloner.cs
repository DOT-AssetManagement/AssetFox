using System;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services
{
    internal class InvestmentPlanCloner
    {
        internal static InvestmentPlanDTO Clone(InvestmentPlanDTO investmentPlan)
        {
            var clone = new InvestmentPlanDTO
            {
                Id = Guid.NewGuid(),
                MinimumProjectCostLimit = investmentPlan.MinimumProjectCostLimit,
                FirstYearOfAnalysisPeriod = investmentPlan.FirstYearOfAnalysisPeriod,               
                InflationRatePercentage = investmentPlan.InflationRatePercentage,
                NumberOfYearsInAnalysisPeriod = investmentPlan.NumberOfYearsInAnalysisPeriod,
                ShouldAccumulateUnusedBudgetAmounts = investmentPlan.ShouldAccumulateUnusedBudgetAmounts,
            };
            return clone;
        }

    }
}
