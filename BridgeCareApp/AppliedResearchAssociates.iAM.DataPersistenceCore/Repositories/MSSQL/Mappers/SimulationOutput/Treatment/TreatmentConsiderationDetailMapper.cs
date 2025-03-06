using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class TreatmentConsiderationDetailMapper
    {
        public static TreatmentConsiderationDetailEntity ToEntityWithoutChildren(
            this TreatmentConsiderationDetail domain,
            Guid assetDetailId)
        {
            var id = Guid.NewGuid();
            var entity = new TreatmentConsiderationDetailEntity
            {
                Id = id,
                AssetDetailId = assetDetailId,
                BudgetPriorityLevel = domain.BudgetPriorityLevel,
                TreatmentName = domain.TreatmentName,
            };
            return entity;
        }

        public static List<TreatmentConsiderationDetailEntity> ToEntityList(
            List<TreatmentConsiderationDetail> domainList,
            Guid assetDetailId)
        {
            var entityList = new List<TreatmentConsiderationDetailEntity>();
            foreach (var domain in domainList)
            {
                var entity = ToEntityWithoutChildren(domain, assetDetailId);
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

            // TODO for reports
            // FundingCalculationInput


            // FundingCalculationOutput


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
            List<TreatmentConsiderationDetail> treatmentConsiderations
            )
        {
            foreach (var treatmentConsideration in treatmentConsiderations)
            {
                var entity = ToEntityWithoutChildren(treatmentConsideration, assetDetailId);
                
                var cashFlowConsiderations = CashFlowConsiderationDetailMapper.ToEntityList(treatmentConsideration.CashFlowConsiderations, entity.Id);
                family.CashFlowConsiderations.AddRange(cashFlowConsiderations);

                // FundingCalculationInput
                var fundingCalculationInput = FundingCalculationInputMapper.ToEntity(treatmentConsideration.FundingCalculationInput, entity.Id, family);
                entity.FundingCalculationInputId = fundingCalculationInput.Id;
                family.FundingCalculationInputs.Add(fundingCalculationInput);

                // FundingCalculationOutput
                var fundingCalculationOutput = FundingCalculationOutputMapper.ToEntity(treatmentConsideration.FundingCalculationOutput, entity.Id, family);
                entity.FundingCalculationOutputId = fundingCalculationOutput.Id;
                family.FundingCalculationOutputs.Add(fundingCalculationOutput);

                family.TreatmentConsiderations.Add(entity);
            }
        }
    }
}
