using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Treatment;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Enums;
using MoreLinq;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DTOs.Static;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class SelectableTreatmentMapper
    {
        public static SelectableTreatmentEntity ToLibraryEntity(this TreatmentDTO dto, Guid libraryId) =>
            new SelectableTreatmentEntity
        {
                Id = dto.Id,
                TreatmentLibraryId = libraryId,
                Name = dto.Name,
                ShadowForAnyTreatment = dto.ShadowForAnyTreatment,
                ShadowForSameTreatment = dto.ShadowForSameTreatment,
                Description = dto.Description,
                Category = (Enums.TreatmentEnum.TreatmentCategory)dto.Category,
                AssetType = dto.AssetType,
                IsUnselectable = dto.IsUnselectable
            };

        public static ScenarioSelectableTreatmentEntity ToScenarioEntity(this TreatmentDTO dto, Guid simulationId, BaseEntityProperties baseEntityProperties=null)
        {
            var treatment = new ScenarioSelectableTreatmentEntity
            {
                Id = dto.Id,
                SimulationId = simulationId,
                Name = dto.Name,
                ShadowForAnyTreatment = dto.ShadowForAnyTreatment,
                ShadowForSameTreatment = dto.ShadowForSameTreatment,
                Description = dto.Description,

                IsModified = dto.IsModified,
                LibraryId = dto.LibraryId,
                IsUnselectable = dto.IsUnselectable,

                Category = (Enums.TreatmentEnum.TreatmentCategory)dto.Category,
                AssetType = dto.AssetType,                
                ScenarioTreatmentPerformanceFactors = dto.ToScenarioSelectableTreatmentPerformanceFactorEntity(baseEntityProperties),
           
            };
            BaseEntityPropertySetter.SetBaseEntityProperties(treatment, baseEntityProperties);
            return treatment;
        }

        public static ScenarioSelectableTreatmentEntity ToScenarioEntityWithCriterionLibraryWithChildren(this TreatmentDTO dto, Guid simulationId, IEnumerable<AttributeEntity> attributes, BaseEntityProperties baseEntityProperties = null)
        {
            var entity = ToScenarioEntity(dto, simulationId);
            var criterionLibraryDto = dto.CriterionLibrary;
            var isvalid = criterionLibraryDto.IsValid();
            if (isvalid)
            {
                var criterionLibrary = criterionLibraryDto.ToSingleUseEntity(baseEntityProperties);
                var join = new CriterionLibraryScenarioSelectableTreatmentEntity
                {
                    ScenarioSelectableTreatmentId = entity.Id,
                    CriterionLibrary = criterionLibrary,
                };
                BaseEntityPropertySetter.SetBaseEntityProperties(entity, baseEntityProperties);
                BaseEntityPropertySetter.SetBaseEntityProperties(join, baseEntityProperties);
                entity.CriterionLibraryScenarioSelectableTreatmentJoin = join;
            }
            var costEntities = new List<ScenarioTreatmentCostEntity>();
            foreach (var cost in dto.Costs)
            {
                var costEntity = cost.ToScenarioEntityWithCriterionLibraryJoin(dto.Id, baseEntityProperties);
                costEntities.Add(costEntity);
            }
            BaseEntityPropertySetter.SetBaseEntityProperties(entity, baseEntityProperties);
            entity.ScenarioTreatmentCosts = costEntities;

            var consequencetEntities = new List<ScenarioConditionalTreatmentConsequenceEntity>();
            foreach (var consequence in dto.Consequences)
            {
                var attributeName = consequence.Attribute;
                var attribute = attributes.FirstOrDefault(a => a.Name == attributeName);
                var consequenceEntity = consequence.ToScenarioEntityWithCriterionLibraryJoin(dto.Id, attribute.Id, baseEntityProperties);
                consequencetEntities.Add(consequenceEntity);
            }
            BaseEntityPropertySetter.SetBaseEntityProperties(entity, baseEntityProperties);
            entity.ScenarioTreatmentConsequences = consequencetEntities;

            var supersedeRuleEntities = new List<ScenarioTreatmentSupersedeRuleEntity>();
            if (dto.SupersedeRules != null)
            {
                foreach (var supersedeRule in dto.SupersedeRules)
                {
                    var supersedeRuleEntity = supersedeRule.ToScenarioTreatmentSupersedeRuleEntity(dto.Id, baseEntityProperties);
                    supersedeRuleEntities.Add(supersedeRuleEntity);
                }                
            }
            BaseEntityPropertySetter.SetBaseEntityProperties(entity, baseEntityProperties);
            entity.ScenarioTreatmentSupersedeRules = supersedeRuleEntities;

            return entity;
        }

        public static ScenarioSelectableTreatmentEntity ToScenarioEntity(this Treatment domain, Guid simulationId) =>
            new ScenarioSelectableTreatmentEntity
            {
                Id = domain.Id,
                SimulationId = simulationId,
                Name = domain.Name,
                ShadowForAnyTreatment = domain.ShadowForAnyTreatment,
                ShadowForSameTreatment = domain.ShadowForSameTreatment,
            };

        public static TreatmentLibraryEntity ToEntity(this TreatmentLibraryDTO dto) =>
            new TreatmentLibraryEntity { Id = dto.Id, Name = dto.Name, Description = dto.Description, IsShared = dto.IsShared };

        public static SelectableTreatment CreateSelectableTreatment(this ScenarioSelectableTreatmentEntity entity, Simulation simulation, List<ScenarioSelectableTreatmentEntity> simpleTreatments, List<ScenarioTreatmentRuleOverrideCriterionEntity> activeOverrideCriteria = null)
        {
            var selectableTreatment = simulation.AddTreatment();
            PopulateSelectableTreatment(entity, selectableTreatment, simulation, simpleTreatments, activeOverrideCriteria);

            if (selectableTreatment.Name == "No Treatment")
            {
                selectableTreatment.DesignateAsPassiveForSimulation();
            }
            return selectableTreatment;
        }

        public static SelectableTreatment ToDomain(this ScenarioSelectableTreatmentEntity entity, Simulation simulation, List<ScenarioSelectableTreatmentEntity> simpleTreatments, List<ScenarioTreatmentRuleOverrideCriterionEntity> activeOverrideCriteria = null)
        {
            var selectableTreatment = new SelectableTreatment(simulation);
            PopulateSelectableTreatment(entity, selectableTreatment, simulation, simpleTreatments, activeOverrideCriteria);
            return selectableTreatment;
        }

        private static void PopulateSelectableTreatment(ScenarioSelectableTreatmentEntity entity, SelectableTreatment selectableTreatment, Simulation simulation, List<ScenarioSelectableTreatmentEntity> simpleTreatments, List<ScenarioTreatmentRuleOverrideCriterionEntity> activeOverrideCriteria = null)
        {
            selectableTreatment.Id = entity.Id;
            selectableTreatment.Name = entity.Name;
            selectableTreatment.SetShadowForAnyTreatment(entity.ShadowForAnyTreatment);
            selectableTreatment.SetShadowForSameTreatment(entity.ShadowForSameTreatment);
            selectableTreatment.Description = entity.Description;
            selectableTreatment.Category = (TreatmentCategory)entity.Category;
            selectableTreatment.AssetCategory = entity.AssetType;
            selectableTreatment.ForCommittedProjectsOnly = entity.IsUnselectable;

            if (entity.ScenarioSelectableTreatmentScenarioBudgetJoins.Any())
            {
                var budgetIds = entity.ScenarioSelectableTreatmentScenarioBudgetJoins
                    .Select(_ => _.ScenarioBudget.Id)
                    .ToList();

                simulation.InvestmentPlan.Budgets
                    .Where(_ => budgetIds.Contains(_.Id))
                    .ToList()
                    .ForEach(budget => selectableTreatment.Budgets.Add(budget));
            }

            if (entity.ScenarioTreatmentPerformanceFactors.Any())
            {
                entity.ScenarioTreatmentPerformanceFactors.ForEach(_ =>
                {
                    var numberAttributes = simulation.Network.Explorer.NumberAttributes;
                    foreach (var attribute in numberAttributes)
                    {
                        if (attribute.Name == _.Attribute)
                        {
                            selectableTreatment.PerformanceCurveAdjustmentFactors.Add(attribute, _.PerformanceFactor);
                        }
                    }
                });
            }

            if (entity.ScenarioTreatmentConsequences.Any())
            {
                entity.ScenarioTreatmentConsequences.ForEach(_ => _.CreateConditionalTreatmentConsequence(selectableTreatment, simulation.Network.Explorer.AllAttributes));
            }

            if (entity.ScenarioTreatmentCosts.Any())
            {
                entity.ScenarioTreatmentCosts.ForEach(_ => _.CreateTreatmentCost(selectableTreatment));
            }

            var baseExpression = entity.CriterionLibraryScenarioSelectableTreatmentJoin?.CriterionLibrary.MergedCriteriaExpression ?? string.Empty;

            baseExpression = CombineOverrideCriteria(baseExpression, activeOverrideCriteria?
                .Where(c => c.TreatmentJoins != null && c.TreatmentJoins.Any(t => t.ScenarioSelectableTreatmentId == entity.Id)));

            var feasibility = selectableTreatment.AddFeasibilityCriterion();
            feasibility.Expression = baseExpression;

            if (entity.ScenarioTreatmentSchedulings.Any())
            {
                entity.ScenarioTreatmentSchedulings.ForEach(_ => _.CreateTreatmentScheduling(selectableTreatment, simulation));
            }

            if (entity.ScenarioTreatmentSupersedeRules.Any())
            {
                entity.ScenarioTreatmentSupersedeRules.ForEach(_ => _.CreateTreatmentSupersedeRule(selectableTreatment, simulation, simpleTreatments));
            }
        }

        public static TreatmentDTO ToDto(this SelectableTreatmentEntity entity, List<TreatmentDTO> treatmentList = null)
        {
            var result = new TreatmentDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                ShadowForAnyTreatment = entity.ShadowForAnyTreatment,
                ShadowForSameTreatment = entity.ShadowForSameTreatment,
                BudgetIds = new List<Guid>(),
                PerformanceFactors = entity.TreatmentPerformanceFactors.Any()
                     ? entity.TreatmentPerformanceFactors.Select(_ => _.ToDto()).ToList()
                     : new List<TreatmentPerformanceFactorDTO>(),
                Costs = entity.TreatmentCosts.Any()
                     ? entity.TreatmentCosts.Select(_ => _.ToDto()).ToList()
                     : new List<TreatmentCostDTO>(),
                Consequences = entity.TreatmentConsequences.Any()
                     ? entity.TreatmentConsequences.Select(_ => _.ToDto()).ToList()
                     : new List<TreatmentConsequenceDTO>(),
                CriterionLibrary = entity.CriterionLibrarySelectableTreatmentJoin != null
                     ? entity.CriterionLibrarySelectableTreatmentJoin.CriterionLibrary.ToDto()
                     : new CriterionLibraryDTO(),
                Category = (TreatmentCategory)entity.Category,
                AssetType = string.Join(", ", entity.AssetType),
                IsUnselectable = entity.IsUnselectable,
                SupersedeRules = new List<TreatmentSupersedeRuleDTO>()
            };

            if (treatmentList != null)
            {
                result.SupersedeRules = entity.TreatmentSupersedeRules.Any()
                                ? entity.TreatmentSupersedeRules.Select(_ => _.ToDto(treatmentList)).ToList()
                                : new List<TreatmentSupersedeRuleDTO>();
            }

            return result;
        }

        public static TreatmentDTOWithSimulationId ToDtoWithSimulationId(this ScenarioSelectableTreatmentEntity entity, List<TreatmentDTO> treatmentList = null)
        {
            var treatmentDto = entity.ToDto(treatmentList);
            return new TreatmentDTOWithSimulationId
            {
                SimulationId
                = entity.SimulationId,
                Treatment = treatmentDto,
            };
        }

        public static List<ScenarioTreatmentPerformanceFactorEntity> ToScenarioSelectableTreatmentPerformanceFactorEntity(this TreatmentDTO dto, BaseEntityProperties baseEntityProperties)
        {
            List<ScenarioTreatmentPerformanceFactorEntity> treatmentPerformanceFactors = new List<ScenarioTreatmentPerformanceFactorEntity>();
            // need to return a list of scenariotretmentperformancefactorentities
            // how do i get the attribute from the dto?
            dto.PerformanceFactors.ForEach(p =>
            {
                var treatmentPerformanceFactor = new ScenarioTreatmentPerformanceFactorEntity()
                {
                    Attribute = p.Attribute,
                    PerformanceFactor = p.PerformanceFactor
                };
                BaseEntityPropertySetter.SetBaseEntityProperties(treatmentPerformanceFactor, baseEntityProperties);
                treatmentPerformanceFactors.Add(treatmentPerformanceFactor);
            });
            return treatmentPerformanceFactors;
        }

        public static TreatmentLibraryDTO ToDto(this TreatmentLibraryEntity entity, List<TreatmentDTO> treatmentList = null) =>
            new TreatmentLibraryDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Owner = entity.CreatedBy,
                IsShared = entity.IsShared,
                Treatments = entity.Treatments.Any()
                    ? entity.Treatments.Select(_ => _.ToDto(treatmentList)).OrderBy(t => t.Name).ToList()
                    : new List<TreatmentDTO>(),
                RuleOverrides = entity.RuleOverrides != null && entity.RuleOverrides.Any()
                    ? entity.RuleOverrides.Select(_ => _.ToDto()).OrderBy(o => o.Name).ToList()
                    : new List<TreatmentRuleOverrideDTO>()
            };

        public static TreatmentRuleOverrideDTO ToDto(this TreatmentRuleOverrideEntity entity) =>
            new TreatmentRuleOverrideDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                IsEnabledByDefault = entity.IsEnabledByDefault,
                IsEnabled = entity.IsEnabledByDefault,
                Criteria = entity.Criteria != null && entity.Criteria.Any()
                    ? entity.Criteria.OrderBy(_ => _.SortOrder).Select(_ => _.ToDto()).ToList()
                    : new List<TreatmentRuleOverrideCriterionDTO>()
            };

        public static TreatmentRuleOverrideCriterionDTO ToDto(this TreatmentRuleOverrideCriterionEntity entity) =>
            new TreatmentRuleOverrideCriterionDTO
            {
                Id = entity.Id,
                LogicalOperator = NormalizeLogicalOperator(entity.LogicalOperator),
                SortOrder = entity.SortOrder,
                CriterionExpression = entity.CriterionExpression,
                TreatmentIds = entity.TreatmentJoins != null && entity.TreatmentJoins.Any()
                    ? entity.TreatmentJoins.Select(_ => _.SelectableTreatmentId).ToList()
                    : new List<Guid>(),
                Treatments = entity.TreatmentJoins != null && entity.TreatmentJoins.Any()
                    ? entity.TreatmentJoins
                        .Where(_ => _.SelectableTreatment != null)
                        .Select(_ => new SimpleTreatmentDTO
                        {
                            Id = _.SelectableTreatmentId,
                            Name = _.SelectableTreatment.Name,
                            IsUnselectable = _.SelectableTreatment.IsUnselectable
                        })
                        .OrderBy(_ => _.Name)
                        .ToList()
                    : new List<SimpleTreatmentDTO>()
            };

        public static TreatmentRuleOverrideDTO ToDto(this ScenarioTreatmentRuleOverrideEntity entity) =>
            new TreatmentRuleOverrideDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                SourceTreatmentRuleOverrideId = entity.SourceTreatmentRuleOverrideId,
                IsEnabledByDefault = entity.IsEnabled,
                IsEnabled = entity.IsEnabled,
                Criteria = entity.Criteria != null && entity.Criteria.Any()
                    ? entity.Criteria.OrderBy(_ => _.SortOrder).Select(_ => _.ToDto()).ToList()
                    : new List<TreatmentRuleOverrideCriterionDTO>()
            };

        public static TreatmentRuleOverrideCriterionDTO ToDto(this ScenarioTreatmentRuleOverrideCriterionEntity entity) =>
            new TreatmentRuleOverrideCriterionDTO
            {
                Id = entity.Id,
                LogicalOperator = NormalizeLogicalOperator(entity.LogicalOperator),
                SortOrder = entity.SortOrder,
                CriterionExpression = entity.CriterionExpression,
                TreatmentIds = entity.TreatmentJoins != null && entity.TreatmentJoins.Any()
                    ? entity.TreatmentJoins.Select(_ => _.ScenarioSelectableTreatmentId).ToList()
                    : new List<Guid>(),
                Treatments = entity.TreatmentJoins != null && entity.TreatmentJoins.Any()
                    ? entity.TreatmentJoins
                        .Where(_ => _.ScenarioSelectableTreatment != null)
                        .Select(_ => new SimpleTreatmentDTO
                        {
                            Id = _.ScenarioSelectableTreatmentId,
                            Name = _.ScenarioSelectableTreatment.Name,
                            IsUnselectable = _.ScenarioSelectableTreatment.IsUnselectable
                        })
                        .OrderBy(_ => _.Name)
                        .ToList()
                    : new List<SimpleTreatmentDTO>()
            };

        public static ScenarioTreatmentRuleOverrideEntity ToScenarioEntity(this TreatmentRuleOverrideDTO dto, Guid simulationId, BaseEntityProperties baseEntityProperties = null)
        {
            var entity = new ScenarioTreatmentRuleOverrideEntity
            {
                Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
                SimulationId = simulationId,
                SourceTreatmentRuleOverrideId = dto.SourceTreatmentRuleOverrideId,
                Name = dto.Name,
                Description = dto.Description,
                IsEnabled = dto.IsEnabled
            };
            BaseEntityPropertySetter.SetBaseEntityProperties(entity, baseEntityProperties);

            foreach (var criterionDto in dto.Criteria ?? new List<TreatmentRuleOverrideCriterionDTO>())
            {
                var criterionEntity = new ScenarioTreatmentRuleOverrideCriterionEntity
                {
                    Id = criterionDto.Id != Guid.Empty ? criterionDto.Id : Guid.NewGuid(),
                    ScenarioTreatmentRuleOverrideId = entity.Id,
                    LogicalOperator = NormalizeLogicalOperator(criterionDto.LogicalOperator),
                    SortOrder = criterionDto.SortOrder,
                    CriterionExpression = criterionDto.CriterionExpression
                };
                BaseEntityPropertySetter.SetBaseEntityProperties(criterionEntity, baseEntityProperties);

                foreach (var treatmentId in criterionDto.TreatmentIds?.Where(id => id != Guid.Empty).Distinct() ?? Enumerable.Empty<Guid>())
                {
                    var treatmentJoin = new ScenarioTreatmentRuleOverrideCriterionTreatmentEntity
                    {
                        ScenarioTreatmentRuleOverrideCriterionId = criterionEntity.Id,
                        ScenarioSelectableTreatmentId = treatmentId
                    };
                    BaseEntityPropertySetter.SetBaseEntityProperties(treatmentJoin, baseEntityProperties);
                    criterionEntity.TreatmentJoins.Add(treatmentJoin);
                }

                entity.Criteria.Add(criterionEntity);
            }

            return entity;
        }

        private static string CombineOverrideCriteria(string baseExpression, IEnumerable<ScenarioTreatmentRuleOverrideCriterionEntity> overrideCriteria)
        {
            var baseCriteriaExpression = baseExpression?.Trim() ?? string.Empty;
            if (overrideCriteria == null)
            {
                return baseCriteriaExpression;
            }

            var criteria = overrideCriteria
                .Where(overrideCriterion => !string.IsNullOrWhiteSpace(overrideCriterion.CriterionExpression))
                .Select(overrideCriterion => new
                {
                    LogicalOperator = NormalizeLogicalOperator(overrideCriterion.LogicalOperator),
                    CriterionExpression = ParenthesizeExpression(overrideCriterion.CriterionExpression)
                })
                .ToList();
            if (!criteria.Any())
            {
                return baseCriteriaExpression;
            }

            var combinedExpression = string.IsNullOrWhiteSpace(baseCriteriaExpression)
                ? string.Empty
                : ParenthesizeExpression(baseCriteriaExpression);
            foreach (var criterion in criteria)
            {
                combinedExpression = string.IsNullOrWhiteSpace(combinedExpression)
                    ? criterion.CriterionExpression
                    : $"({combinedExpression} {criterion.LogicalOperator} {criterion.CriterionExpression})";
            }

            return combinedExpression;
        }

        private static string ParenthesizeExpression(string expression)
        {
            var trimmedExpression = expression?.Trim() ?? string.Empty;
            return string.IsNullOrWhiteSpace(trimmedExpression)
                ? string.Empty
                : $"({trimmedExpression})";
        }

        private static string NormalizeLogicalOperator(string logicalOperator)
        {
            if (string.Equals(logicalOperator, "OR", StringComparison.OrdinalIgnoreCase))
            {
                return "OR";
            }

            if (string.Equals(logicalOperator, "AND", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(logicalOperator))
            {
                return "AND";
            }

            throw new InvalidOperationException("Treatment override criteria only support AND or OR append operators.");
        }

        public static TreatmentDTO ToDtoNullSafe(this SelectableTreatmentEntity entity)
        {
            if (entity == null)
            {
                return null;
            }
            return entity.ToDto();
        }

        public static TreatmentDTO ToDto(this ScenarioSelectableTreatmentEntity entity, List<TreatmentDTO> treatmentList = null)
        {
            var result = new TreatmentDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                BudgetIds = entity.ScenarioSelectableTreatmentScenarioBudgetJoins.Any()
                         ? entity.ScenarioSelectableTreatmentScenarioBudgetJoins.Select(_ => _.ScenarioBudgetId).ToList()
                         : new List<Guid>(),
                Budgets = entity.ScenarioSelectableTreatmentScenarioBudgetJoins.Any()
                         ? entity.ScenarioSelectableTreatmentScenarioBudgetJoins.Select(_ => new TreatmentBudgetDTO
                         { Id = _.ScenarioBudgetId, Name = _.ScenarioBudget?.Name }).ToList()
                         : new List<TreatmentBudgetDTO>(),
                Consequences = entity.ScenarioTreatmentConsequences.Any()
                         ? entity.ScenarioTreatmentConsequences.Select(_ => _.ToDto()).ToList()
                         : new List<TreatmentConsequenceDTO>(),
                Costs = entity.ScenarioTreatmentCosts.Any()
                         ? entity.ScenarioTreatmentCosts.Select(_ => _.ToDto()).ToList()
                         : new List<TreatmentCostDTO>(),
                PerformanceFactors = entity.ScenarioTreatmentPerformanceFactors.Any()
                         ? entity.ScenarioTreatmentPerformanceFactors.Select(_ => _.ToDto()).ToList()
                         : new List<TreatmentPerformanceFactorDTO>(),
                CriterionLibrary = entity.CriterionLibraryScenarioSelectableTreatmentJoin != null
                         ? entity.CriterionLibraryScenarioSelectableTreatmentJoin.CriterionLibrary?.ToDto()
                         : new CriterionLibraryDTO(),
                ShadowForAnyTreatment = entity.ShadowForAnyTreatment,
                ShadowForSameTreatment = entity.ShadowForSameTreatment,
                Category = (TreatmentCategory)entity.Category,
                IsModified = entity.IsModified,
                LibraryId = entity.LibraryId,
                AssetType = string.Join(", ", entity.AssetType),
                IsUnselectable = entity.IsUnselectable,
                SupersedeRules = new List<TreatmentSupersedeRuleDTO>()
            };

            if (treatmentList != null)
            {
                result.SupersedeRules = entity.ScenarioTreatmentSupersedeRules.Any()
                                ? entity.ScenarioTreatmentSupersedeRules.Select(_ => _.ToDto(treatmentList)).ToList()
                                : new List<TreatmentSupersedeRuleDTO>();
            }

            return result;
        }
    }
}
