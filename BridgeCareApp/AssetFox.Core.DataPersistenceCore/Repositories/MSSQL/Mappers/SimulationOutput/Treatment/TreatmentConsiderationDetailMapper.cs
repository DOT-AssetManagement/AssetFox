using System;
using System.Collections.Generic;
using System.Linq;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.DTOs;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class TreatmentConsiderationDetailMapper
    {
        public static TreatmentConsiderationDetailEntity ToEntityWithoutChildren(
            this TreatmentConsiderationDetail domain,
            Guid assetDetailId,
            int runId)
        {
            var id = SequentialGuid.NewGuid();
            var entity = new TreatmentConsiderationDetailEntity
            {
                Id = id,
                AssetDetailId = assetDetailId,
                BudgetPriorityLevel = domain.BudgetPriorityLevel,
                TreatmentName = domain.TreatmentName,
                RunId = runId
            };
            return entity;
        }

        public static List<TreatmentConsiderationDetailEntity> ToEntityList(
            List<TreatmentConsiderationDetail> domainList,
            Guid assetDetailId,
            int runId)
        {
            var entityList = new List<TreatmentConsiderationDetailEntity>();
            foreach (var domain in domainList)
            {
                var entity = ToEntityWithoutChildren(domain, assetDetailId, runId);
                entityList.Add(entity);
            }
            return entityList;
        }

        private static TreatmentConsiderationDetail ToDomain(TreatmentConsiderationDetailEntity entity)
        {
            var domain = new TreatmentConsiderationDetail(entity.TreatmentName)
            {
                BudgetPriorityLevel = entity.BudgetPriorityLevel,
            };
            var cashFlowConsiderations = CashFlowConsiderationDetailMapper.ToDomainList(entity.CashFlowConsiderations);
            domain.CashFlowConsiderations.AddRange(cashFlowConsiderations);

            // FundingCalculationInput
            var fundingCalculationInput = FundingCalculationInputMapper.ToDomain(entity.FundingCalculationInput);
            domain.FundingCalculationInput = fundingCalculationInput;

            // FundingCalculationOutput
            var fundingCalculationOutput = FundingCalculationOutputMapper.ToDomain(entity.FundingCalculationOutput);
            domain.FundingCalculationOutput = fundingCalculationOutput;

            return domain;
        }

        public static List<TreatmentConsiderationDetail> ToDomainList(ICollection<TreatmentConsiderationDetailEntity> entityCollection)
        {
            var domainList = new List<TreatmentConsiderationDetail>();
            foreach (var entity in entityCollection)
            {
                var domain = ToDomain(entity);
                domainList.Add(domain);
            }
            return domainList;
        }

        public static void AddToFamily(
            Guid assetDetailId,
            AssetDetailEntityFamily family,
            List<TreatmentConsiderationDetail> treatmentConsiderations,
            int runId
            )
        {
            foreach (var treatmentConsideration in treatmentConsiderations)
            {
                var entity = ToEntityWithoutChildren(treatmentConsideration, assetDetailId, runId);
                
                var cashFlowConsiderations = CashFlowConsiderationDetailMapper.ToEntityList(treatmentConsideration.CashFlowConsiderations, entity.Id, runId);
                family.CashFlowConsiderations.AddRange(cashFlowConsiderations);

                // FundingCalculationInput
                var fundingCalculationInput = FundingCalculationInputMapper.ToEntity(treatmentConsideration.FundingCalculationInput, entity.Id, family, runId);
                family.FundingCalculationInputs.Add(fundingCalculationInput);

                // FundingCalculationOutput
                var fundingCalculationOutput = FundingCalculationOutputMapper.ToEntity(treatmentConsideration.FundingCalculationOutput, entity.Id, family, runId);
                family.FundingCalculationOutputs.Add(fundingCalculationOutput);

                family.TreatmentConsiderations.Add(entity);
            }
        }

        public static TreatmentConsiderationDetailDTO ToDto(this TreatmentConsiderationDetailEntity entity)
        {
            var dto = new TreatmentConsiderationDetailDTO
            {
                Id = entity.Id,
                BudgetPriorityLevel = entity.BudgetPriorityLevel,
                TreatmentName = entity.TreatmentName,
                CashFlowConsiderations = entity.CashFlowConsiderations.Select(_ => _.ToDto()).ToList(),
                FundingCalculationInput = entity.FundingCalculationInput.ToDto(),
                FundingCalculationOutput = entity.FundingCalculationOutput.ToDto()
            };

            return dto;
        }

        public static TreatmentConsiderationDetailEntity ToEntity(this TreatmentConsiderationDetailDTO treatmentConsiderationDetailDto, Guid assetDetailId)
        {
            var treatmentConsiderationDetailId = treatmentConsiderationDetailDto.Id;

            return new TreatmentConsiderationDetailEntity
            {
                Id = treatmentConsiderationDetailId,
                AssetDetailId = assetDetailId,
                BudgetPriorityLevel = treatmentConsiderationDetailDto.BudgetPriorityLevel,
                TreatmentName = treatmentConsiderationDetailDto.TreatmentName,
                CashFlowConsiderations = treatmentConsiderationDetailDto.CashFlowConsiderations.Select(_ => _.ToEntity(treatmentConsiderationDetailId)).ToList(),
                FundingCalculationInput = treatmentConsiderationDetailDto.FundingCalculationInput.ToEntity(treatmentConsiderationDetailId),
                FundingCalculationOutput = treatmentConsiderationDetailDto.FundingCalculationOutput.ToEntity(treatmentConsiderationDetailId)
            };
        }
    }
}
