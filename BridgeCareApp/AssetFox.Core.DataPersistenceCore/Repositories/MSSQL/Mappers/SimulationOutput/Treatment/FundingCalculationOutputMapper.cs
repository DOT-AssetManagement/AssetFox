using System;
using System.Linq;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class FundingCalculationOutputMapper
    {
        public static FundingCalculationOutput ToEntityWithoutChildren(
            Guid treatmentConsiderationDetailEntityId, int runId)
        {
            return new FundingCalculationOutput
            {
                Id = SequentialGuid.NewGuid(),
                TreatmentConsiderationDetailId = treatmentConsiderationDetailEntityId,
                RunId = runId
            };
        }

        public static FundingCalculationOutput ToEntity(Analysis.Engine.FundingCalculationOutput fundingCalculationOutputDomain, Guid treatmentConsiderationDetailEntityId, AssetDetailEntityFamily family, int runId)
        {
            var entity = ToEntityWithoutChildren(treatmentConsiderationDetailEntityId, runId);

            // AllocationMatrix
            var allocationMatrix = AllocationMapper.ToEntityList(fundingCalculationOutputDomain?.AllocationMatrix ?? new(), entity.Id, runId);
            family.AllocationMatrix.AddRange(allocationMatrix);

            return entity;
        }

        public static Analysis.Engine.FundingCalculationOutput ToDomain(FundingCalculationOutput fundingCalculationOutputEntity)
        {
            var domain = new Analysis.Engine.FundingCalculationOutput();
            var allocationMatrix = AllocationMapper.ToDomainList(fundingCalculationOutputEntity?.AllocationMatrix);
            domain.AllocationMatrix.AddRange(allocationMatrix);

            return domain;
        }

        public static FundingCalculationOutputDTO ToDto(this FundingCalculationOutput entity)
        {
            var dto = new FundingCalculationOutputDTO
            {
                Id = entity.Id,
                AllocationMatrix = entity.AllocationMatrix.Select(_ => _.ToDto()).ToList()
            };

            return dto;
        }

        public static FundingCalculationOutput ToEntity(this FundingCalculationOutputDTO fundingCalculationOutputDto, Guid treatmentConsiderationDetailId)
        {
            var fundingCalculationOutputId = fundingCalculationOutputDto.Id;

            return new FundingCalculationOutput
            {
                Id = fundingCalculationOutputId,
                TreatmentConsiderationDetailId = treatmentConsiderationDetailId,
                AllocationMatrix = fundingCalculationOutputDto.AllocationMatrix.Select(_ => _.ToEntity(fundingCalculationOutputId)).ToList()
            };
        }
    }
}
