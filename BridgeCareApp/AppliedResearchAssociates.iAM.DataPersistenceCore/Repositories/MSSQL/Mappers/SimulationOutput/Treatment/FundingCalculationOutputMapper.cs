using System;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class FundingCalculationOutputMapper
    {
        public static Entities.FundingCalculationOutput ToEntityWithoutChildren(
            Guid treatmentConsiderationDetailEntityId)
        {
            return new Entities.FundingCalculationOutput
            {
                Id = Guid.NewGuid(),
                TreatmentConsiderationDetailId = treatmentConsiderationDetailEntityId
            };
        }

        public static Entities.FundingCalculationOutput ToEntity(Analysis.Engine.FundingCalculationOutput fundingCalculationOutput, Guid treatmentConsiderationDetailEntityId, AssetDetailEntityFamily family)
        {
            var entity = ToEntityWithoutChildren(treatmentConsiderationDetailEntityId);

            // AllocationMatrix
            var allocationMatrix = AllocationMapper.ToEntityList(fundingCalculationOutput.AllocationMatrix, entity.Id);
            family.AllocationMatrix.AddRange(allocationMatrix);

            return entity;
        }
    }
}
