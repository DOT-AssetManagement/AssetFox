using System;
using System.Linq;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class FundingCalculationInputMapper
    {
        public static FundingCalculationInput ToEntityWithoutChildren(
            Guid treatmentConsiderationDetailEntityId)
        {
            return new FundingCalculationInput
            {
                Id = Guid.NewGuid(),
                TreatmentConsiderationDetailId = treatmentConsiderationDetailEntityId
            };
        }

        public static FundingCalculationInput ToEntity(Analysis.Engine.FundingCalculationInput fundingCalculationInputDomain, Guid treatmentConsiderationDetailEntityId, AssetDetailEntityFamily family)
        {
            var entity = ToEntityWithoutChildren(treatmentConsiderationDetailEntityId);

            // CurrentBudgetsToSpend
            var currentBudgetsToSpend = BudgetToSpendMapper.ToEntityList(fundingCalculationInputDomain?.CurrentBudgetsToSpend ?? new(), entity.Id);
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
    }
}
