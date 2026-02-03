using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.CashFlow;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.CashFlow;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class CashFlowDistributionRuleMapper
    {
        public static ScenarioCashFlowDistributionRuleEntity ToScenarioEntity(this CashFlowDistributionRule domain, Guid cashFlowRuleId, int durationInYears) =>
            new ScenarioCashFlowDistributionRuleEntity
            {
                Id = domain.Id,
                ScenarioCashFlowRuleId = cashFlowRuleId,
                DurationInYears = durationInYears,
                YearlyPercentages = string.Join('/', domain.YearlyPercentages),
                CostCeiling = domain.CostCeiling ?? 0
            };

        public static ScenarioCashFlowDistributionRuleEntity ToScenarioEntity(this CashFlowDistributionRuleDTO dto, Guid cashFlowRuleId, BaseEntityProperties baseEntityProperties=null)
        {
            var entity = new ScenarioCashFlowDistributionRuleEntity
            {
                Id = dto.Id,
                ScenarioCashFlowRuleId = cashFlowRuleId,
                CostCeiling = dto.CostCeiling,
                DurationInYears = dto.DurationInYears,
                YearlyPercentages = dto.YearlyPercentages
            };
            BaseEntityPropertySetter.SetBaseEntityProperties(entity, baseEntityProperties);
            return entity;
        }
        public static CashFlowDistributionRuleDTO ToDto(this ScenarioCashFlowDistributionRuleEntity entity) =>
            new CashFlowDistributionRuleDTO
            {
                Id = entity.Id,
                CostCeiling = entity.CostCeiling,
                DurationInYears = entity.DurationInYears,
                YearlyPercentages = entity.YearlyPercentages
            };

        public static CashFlowDistributionRuleEntity ToLibraryEntity(this CashFlowDistributionRuleDTO dto, Guid cashFlowRuleId) =>
            new CashFlowDistributionRuleEntity
            {
                Id = dto.Id,
                CashFlowRuleId = cashFlowRuleId,
                CostCeiling = dto.CostCeiling,
                DurationInYears = dto.DurationInYears,
                YearlyPercentages = dto.YearlyPercentages
            };

        public static CashFlowDistributionRuleDTO ToDto(this CashFlowDistributionRuleEntity entity) =>
            new CashFlowDistributionRuleDTO
            {
                Id = entity.Id,
                CostCeiling = entity.CostCeiling,
                DurationInYears = entity.DurationInYears,
                YearlyPercentages = entity.YearlyPercentages
            };
    }
}
