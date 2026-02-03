using System;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.SimulationCloning
{
    public class FundingCalculationInputCloner
    {
        internal static FundingCalculationInputDTO Clone(FundingCalculationInputDTO fundingCalculationInput)
        {
            var cloneCurrentBudgetsToSpend = BudgetToSpendCloner.CloneList(fundingCalculationInput.CurrentBudgetsToSpend);

            return new FundingCalculationInputDTO
            {
                Id = Guid.NewGuid(),
                CurrentBudgetsToSpend = cloneCurrentBudgetsToSpend
            };
        }
    }
}
