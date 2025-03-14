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

        public static Entities.FundingCalculationOutput ToEntity(Analysis.Engine.FundingCalculationOutput fundingCalculationOutputDomain, Guid treatmentConsiderationDetailEntityId, AssetDetailEntityFamily family)
        {
            var entity = ToEntityWithoutChildren(treatmentConsiderationDetailEntityId);

            // AllocationMatrix
            var allocationMatrix = AllocationMapper.ToEntityList(fundingCalculationOutputDomain?.AllocationMatrix ?? new(), entity.Id);
            family.AllocationMatrix.AddRange(allocationMatrix);

            return entity;
        }

        public static Analysis.Engine.FundingCalculationOutput ToDomain(Entities.FundingCalculationOutput fundingCalculationOutputEntity)
        {
            var domain = new Analysis.Engine.FundingCalculationOutput();
            var allocationMatrix = AllocationMapper.ToDomainList(fundingCalculationOutputEntity?.AllocationMatrix);
            domain.AllocationMatrix.AddRange(allocationMatrix);

            return domain;
        }
    }
}
