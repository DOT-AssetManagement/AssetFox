using System;
using AppliedResearchAssociates.iAM.DTOs;

namespace BridgeCareCore.Services.SimulationCloning
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
