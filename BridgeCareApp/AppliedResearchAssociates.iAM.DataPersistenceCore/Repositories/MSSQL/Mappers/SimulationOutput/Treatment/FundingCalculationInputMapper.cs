using System;
using System.Linq;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class FundingCalculationInputMapper
    {
        public static FundingCalculationInput ToEntityWithoutChildren(
            Guid treatmentConsiderationDetailEntityId, int runId)
        {
            return new FundingCalculationInput
            {
                Id = Guid.NewGuid(),
                TreatmentConsiderationDetailId = treatmentConsiderationDetailEntityId,
                RunId = runId
            };
        }

        public static FundingCalculationInput ToEntity(Analysis.Engine.FundingCalculationInput fundingCalculationInputDomain, Guid treatmentConsiderationDetailEntityId, AssetDetailEntityFamily family, int runId)
        {
            var entity = ToEntityWithoutChildren(treatmentConsiderationDetailEntityId, runId);

            // CurrentBudgetsToSpend
            var currentBudgetsToSpend = BudgetToSpendMapper.ToEntityList(fundingCalculationInputDomain?.CurrentBudgetsToSpend ?? new(), entity.Id, runId);
            family.CurrentBudgetsToSpend.AddRange(currentBudgetsToSpend);

            return entity;
        }

        public static Analysis.Engine.FundingCalculationInput ToDomain(FundingCalculationInput fundingCalculationInputEntity)
        {
            var domain = new Analysis.Engine.FundingCalculationInput();
            var currentBudgetsToSpend = BudgetToSpendMapper.ToDomainList(fundingCalculationInputEntity?.CurrentBudgetsToSpend);
            domain.CurrentBudgetsToSpend.AddRange(currentBudgetsToSpend);

            return domain;
        }

        public static FundingCalculationInputDTO ToDto(this FundingCalculationInput entity)
        {
            var dto = new FundingCalculationInputDTO
            {
                Id = entity.Id,
                CurrentBudgetsToSpend = entity.CurrentBudgetsToSpend.Select(_ => _.ToDto()).ToList()
            };

            return dto;
        }

        public static FundingCalculationInput ToEntity(this FundingCalculationInputDTO fundingCalculationInputDto, Guid treatmentConsiderationDetailId)
        {
            var fundingCalculationInputId = fundingCalculationInputDto.Id;

            return new FundingCalculationInput
            {
                Id = fundingCalculationInputId,
                TreatmentConsiderationDetailId = treatmentConsiderationDetailId,
                CurrentBudgetsToSpend = fundingCalculationInputDto.CurrentBudgetsToSpend.Select(_ => _.ToEntity(fundingCalculationInputId)).ToList()
            };
        }
    }
}
