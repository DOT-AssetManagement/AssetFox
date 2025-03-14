using System;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class FundingCalculationInputMapper
    {
        public static Entities.FundingCalculationInput ToEntityWithoutChildren(
            Guid treatmentConsiderationDetailEntityId)
        {
            return new Entities.FundingCalculationInput
            {
                Id = Guid.NewGuid(),
                TreatmentConsiderationDetailId = treatmentConsiderationDetailEntityId
            };
        }

        public static Entities.FundingCalculationInput ToEntity(Analysis.Engine.FundingCalculationInput fundingCalculationInputDomain, Guid treatmentConsiderationDetailEntityId, AssetDetailEntityFamily family)
        {
            var entity = ToEntityWithoutChildren(treatmentConsiderationDetailEntityId);

            // CurrentBudgetsToSpend
            var currentBudgetsToSpend = BudgetToSpendMapper.ToEntityList(fundingCalculationInputDomain?.CurrentBudgetsToSpend ?? new(), entity.Id);
            family.CurrentBudgetsToSpend.AddRange(currentBudgetsToSpend);

            return entity;
        }

        public static Analysis.Engine.FundingCalculationInput ToDomain(Entities.FundingCalculationInput fundingCalculationInputEntity)
        {
            var domain = new Analysis.Engine.FundingCalculationInput();
            var currentBudgetsToSpend = BudgetToSpendMapper.ToDomainList(fundingCalculationInputEntity?.CurrentBudgetsToSpend);
            domain.CurrentBudgetsToSpend.AddRange(currentBudgetsToSpend);

            return domain;
        }
    }
}
