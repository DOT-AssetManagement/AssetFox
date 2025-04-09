using System;
using System.Collections.Generic;
using System.Linq;
using AnalysisEngine = AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class BudgetToSpendMapper
    {
        public static IEnumerable<BudgetToSpend> ToEntityList(List<AnalysisEngine.FundingCalculationInput.Budget> currentBudgetsToSpendDomainList, Guid fundingCalculationInputId)
        {
            return currentBudgetsToSpendDomainList.Select(_ => new BudgetToSpend
            {
                Id = Guid.NewGuid(),
                Amount = _.Amount,
                Year = _.Year,
                FundingCalculationInputId = fundingCalculationInputId,
                Name = _.Name
            });
        }

        public static IEnumerable<AnalysisEngine.FundingCalculationInput.Budget> ToDomainList(ICollection<BudgetToSpend> currentBudgetsToSpendEntityCollection)
        {
            return currentBudgetsToSpendEntityCollection?.Select(_ => ToDomain(_)).ToList() ?? new();
        }

        private static AnalysisEngine.FundingCalculationInput.Budget ToDomain(BudgetToSpend budgetToSpendEntity)
        {
            return new AnalysisEngine.FundingCalculationInput.Budget(budgetToSpendEntity.Name, budgetToSpendEntity.Amount, budgetToSpendEntity.Year);
        }
    }
}
