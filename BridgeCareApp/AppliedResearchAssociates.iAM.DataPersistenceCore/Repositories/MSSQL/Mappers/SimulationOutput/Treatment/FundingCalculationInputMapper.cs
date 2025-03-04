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

        public static Entities.FundingCalculationInput ToEntity(Analysis.Engine.FundingCalculationInput fundingCalculationInput, Guid treatmentConsiderationDetailEntityId, AssetDetailEntityFamily family)
        {
            var entity = ToEntityWithoutChildren(treatmentConsiderationDetailEntityId);

            // CurrentBudgetsToSpend
            var currentBudgetsToSpend = BudgetToSpendMapper.ToEntityList(fundingCalculationInput.CurrentBudgetsToSpend, entity.Id);
            family.CurrentBudgetsToSpend.AddRange(currentBudgetsToSpend);

            return entity;
        }
    }
}
