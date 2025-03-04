using System;
using System.Collections.Generic;
using System.Linq;
using AnalysisEngine = AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class BudgetToSpendMapper
    {
        public static IEnumerable<BudgetToSpend> ToEntityList(List<AnalysisEngine.FundingCalculationInput.Budget> currentBudgetsToSpend, Guid fundingCalculationInputId)
        {
            return currentBudgetsToSpend.Select(_ => new BudgetToSpend
            {
                Id = Guid.NewGuid(),
                Amount = _.Amount,
                Year = _.Year,
                FundingCalculationInputId = fundingCalculationInputId,
                Name = _.Name
            });
        }
    }
}
