using System;
using System.Collections.Generic;
using System.Linq;
using AnalysisEngine = AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.DTOs;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class BudgetToSpendMapper
    {
        public static IEnumerable<BudgetToSpend> ToEntityList(List<AnalysisEngine.FundingCalculationInput.Budget> currentBudgetsToSpendDomainList, Guid fundingCalculationInputId, int runId)
        {
            return currentBudgetsToSpendDomainList.Select(_ => new BudgetToSpend
            {
                Id = Guid.NewGuid(),
                Amount = _.Amount,
                Year = _.Year,
                FundingCalculationInputId = fundingCalculationInputId,
                Name = _.Name,
                RunId = runId
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

        public static BudgetToSpendDTO ToDto(this BudgetToSpend entity)
        {
            var dto = new BudgetToSpendDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Amount = entity.Amount,
                Year = entity.Year
            };

            return dto;
        }

        public static BudgetToSpend ToEntity(this BudgetToSpendDTO budgetToSpendDto, Guid fundingCalculationInputId)
        {
            return new BudgetToSpend
            {
                Id = budgetToSpendDto.Id,
                FundingCalculationInputId = fundingCalculationInputId,
                Amount = budgetToSpendDto.Amount,
                Name = budgetToSpendDto.Name,
                Year = budgetToSpendDto.Year
            };
        }
    }
}
