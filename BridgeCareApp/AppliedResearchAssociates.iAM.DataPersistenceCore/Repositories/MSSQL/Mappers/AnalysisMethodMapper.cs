using System;
using System.Linq;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.Analysis;
using AppliedResearchAssociates.iAM.DTOs;
using MoreLinq;
using System.Collections.Generic;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class AnalysisMethodMapper
    {
        public static AnalysisMethodEntity ToEntity(this AnalysisMethod domain, Guid simulationId, Guid? attributeId) =>
            new AnalysisMethodEntity
            {
                Id = domain.Id,
                SimulationId = simulationId,
                AttributeId = attributeId,
                Description = domain.Description,
                OptimizationStrategy = domain.OptimizationStrategy,
                SpendingStrategy = domain.SpendingStrategy,
                ShouldApplyMultipleFeasibleCosts = domain.ShouldApplyMultipleFeasibleCosts,
                ShouldDeteriorateDuringCashFlow = domain.ShouldDeteriorateDuringCashFlow,
                ShouldUseExtraFundsAcrossBudgets = domain.AllowFundingFromMultipleBudgets,
            };

        public static void FillSimulationAnalysisMethod(
            this AnalysisMethodEntity entity,
            Simulation simulation,
            string userCriteria,
            IReadOnlyDictionary<Guid, string> attributeNameLookup)
        {
            simulation.AnalysisMethod.Id = entity.Id;
            simulation.AnalysisMethod.Description = entity.Description;
            simulation.AnalysisMethod.OptimizationStrategy = entity.OptimizationStrategy;
            simulation.AnalysisMethod.SpendingStrategy = entity.SpendingStrategy;
            simulation.AnalysisMethod.ShouldApplyMultipleFeasibleCosts = entity.ShouldApplyMultipleFeasibleCosts;
            simulation.AnalysisMethod.ShouldDeteriorateDuringCashFlow = entity.ShouldDeteriorateDuringCashFlow;
            simulation.AnalysisMethod.AllowFundingFromMultipleBudgets = entity.ShouldUseExtraFundsAcrossBudgets;

            var specifiedFilter = entity.CriterionLibraryAnalysisMethodJoin?.CriterionLibrary.MergedCriteriaExpression ?? string.Empty;
            var combinedCriteria = string.IsNullOrEmpty(specifiedFilter) ? userCriteria : $"({userCriteria}) AND ({specifiedFilter})";
            simulation.AnalysisMethod.Filter.Expression =
                string.IsNullOrEmpty(userCriteria) ? specifiedFilter :
                combinedCriteria;

            if (entity.AttributeId.HasValue)
            {
                var attributeName = attributeNameLookup[entity.AttributeId.Value];
                simulation.AnalysisMethod.Weighting = simulation.Network.Explorer.NumberAttributes
                    .Single(_ => _.Name == attributeName);
            }
            if (entity.Benefit != null)
            {
                simulation.AnalysisMethod.Benefit.Id = entity.Benefit.Id;
                simulation.AnalysisMethod.Benefit.Limit = entity.Benefit.Limit;
                var benefitAttributeName = attributeNameLookup[entity.Benefit.AttributeId];
                simulation.AnalysisMethod.Benefit.Attribute = simulation.Network.Explorer.NumericAttributes
                    .Single(_ => _.Name == benefitAttributeName);
            }

            entity.Simulation.BudgetPriorities
                .ForEach(_ => _.CreateBudgetPriority(simulation));

            entity.Simulation.ScenarioTargetConditionalGoals
                .ForEach(_ => _.CreateTargetConditionGoal(simulation, attributeNameLookup));

            entity.Simulation.ScenarioDeficientConditionGoals
                .ForEach(_ => _.CreateDeficientConditionGoal(simulation, attributeNameLookup));

            entity.Simulation.RemainingLifeLimits
                .ForEach(_ => _.CreateRemainingLifeLimit(simulation, attributeNameLookup));

            simulation.ShouldBundleFeasibleTreatments = entity.shouldAllowMultipleTreatments;
        }

        public static AnalysisMethodEntity ToEntity(this AnalysisMethodDTO dto, Guid simulationId, Guid? attributeId = null, BaseEntityProperties baseEntityProperties = null)
        {
            var entity = new AnalysisMethodEntity
            {
                Id = dto.Id,
                SimulationId = simulationId,
                Description = dto.Description,
                OptimizationStrategy = dto.OptimizationStrategy,
                SpendingStrategy = dto.SpendingStrategy,
                ShouldApplyMultipleFeasibleCosts = dto.ShouldApplyMultipleFeasibleCosts,
                ShouldDeteriorateDuringCashFlow = dto.ShouldDeteriorateDuringCashFlow,
                ShouldUseExtraFundsAcrossBudgets = dto.ShouldUseExtraFundsAcrossBudgets,
                shouldAllowMultipleTreatments = dto.ShouldAllowMultipleTreatments,
                AttributeId = attributeId,

            };
            BaseEntityPropertySetter.SetBaseEntityProperties(entity, baseEntityProperties);
            return entity;
        }


        public static AnalysisMethodEntity ToEntityWithBenefit(this AnalysisMethodDTO dto, Guid simulationId, List<AttributeEntity> attributes, Guid? attributeId = null, BaseEntityProperties baseEntityProperties = null)
        {
            var entity = dto.ToEntity(simulationId, attributeId, baseEntityProperties);
            var benefit = dto.Benefit;
            if (benefit != null && benefit.Id != Guid.Empty)
            {
                var benefitAttribute = attributes.First(a => a.Name == benefit.Attribute);
                var benefitEntity = benefit.ToEntity(dto.Id, benefitAttribute.Id, baseEntityProperties);
                entity.Benefit = benefitEntity;
            }
            BaseEntityPropertySetter.SetBaseEntityProperties(entity, baseEntityProperties);
            return entity;
        }

        public static AnalysisMethodDTO ToDto(
            this AnalysisMethodEntity entity,
            IReadOnlyDictionary<Guid, string> attributeNameLookup)
        {
            var attributeName = entity.AttributeId == null ?
                String.Empty
                : attributeNameLookup[entity.AttributeId.Value];
            return new AnalysisMethodDTO
            {
                Id = entity.Id,
                Description = entity.Description,
                OptimizationStrategy = entity.OptimizationStrategy,
                SpendingStrategy = entity.SpendingStrategy,
                ShouldApplyMultipleFeasibleCosts = entity.ShouldApplyMultipleFeasibleCosts,
                ShouldDeteriorateDuringCashFlow = entity.ShouldDeteriorateDuringCashFlow,
                ShouldAllowMultipleTreatments = entity.shouldAllowMultipleTreatments,
                ShouldUseExtraFundsAcrossBudgets = entity.ShouldUseExtraFundsAcrossBudgets,
                Attribute = attributeName,
                Benefit = entity.Benefit?.ToDto(attributeNameLookup) ?? new BenefitDTO(),
                CriterionLibrary = entity.CriterionLibraryAnalysisMethodJoin != null
                    ? entity.CriterionLibraryAnalysisMethodJoin.CriterionLibrary.ToDto()
                    : new CriterionLibraryDTO()
            };
        }
    }
}
