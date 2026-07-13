using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Treatment;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Extensions;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL
{
    public class TreatmentRuleOverrideRepository : ITreatmentRuleOverrideRepository
    {
        private const string AndOperator = "AND";
        private const string OrOperator = "OR";

        private readonly UnitOfDataPersistenceWork _unitOfWork;

        public TreatmentRuleOverrideRepository(UnitOfDataPersistenceWork unitOfWork) =>
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

        public List<TreatmentRuleOverrideDTO> GetRuleOverridesByLibraryId(Guid libraryId)
        {
            var entities = _unitOfWork.Context.TreatmentRuleOverride
                .AsNoTracking()
                .AsSplitQuery()
                .Where(o => o.TreatmentLibraryId == libraryId)
                .Include(o => o.Criteria)
                    .ThenInclude(c => c.TreatmentJoins)
                    .ThenInclude(j => j.SelectableTreatment)
                .OrderBy(o => o.Name)
                .ToList();

            return entities.Select(e => e.ToDto()).ToList();
        }

        public List<TreatmentRuleOverrideDTO> GetRuleOverridesBySimulationId(Guid simulationId)
        {
            var entities = _unitOfWork.Context.ScenarioTreatmentRuleOverride
                .AsNoTracking()
                .AsSplitQuery()
                .Where(o => o.SimulationId == simulationId)
                .Include(o => o.Criteria)
                    .ThenInclude(c => c.TreatmentJoins)
                    .ThenInclude(j => j.ScenarioSelectableTreatment)
                .OrderBy(o => o.Name)
                .ToList();

            return entities.Select(e => e.ToDto()).ToList();
        }

        public void UpsertRuleOverrides(Guid libraryId, List<TreatmentRuleOverrideDTO> overrides)
        {
            overrides ??= new List<TreatmentRuleOverrideDTO>();
            ValidateLibraryRuleOverrides(libraryId, overrides);

            var existingOverrides = _unitOfWork.Context.TreatmentRuleOverride
                .Where(o => o.TreatmentLibraryId == libraryId)
                .Include(o => o.Criteria)
                    .ThenInclude(c => c.TreatmentJoins)
                .ToList();

            var incomingIds = overrides.Where(o => o.Id != Guid.Empty).Select(o => o.Id).ToHashSet();
            var toDelete = existingOverrides.Where(e => !incomingIds.Contains(e.Id)).ToList();
            _unitOfWork.Context.TreatmentRuleOverride.RemoveRange(toDelete);

            foreach (var dto in overrides)
            {
                var existing = existingOverrides.FirstOrDefault(e => e.Id == dto.Id);
                if (existing != null)
                {
                    existing.Name = dto.Name;
                    existing.Description = dto.Description;
                    existing.IsEnabledByDefault = dto.IsEnabledByDefault;

                    ReplaceLibraryCriteria(existing, dto.Criteria);
                }
                else
                {
                    var entity = new TreatmentRuleOverrideEntity
                    {
                        Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
                        TreatmentLibraryId = libraryId,
                        Name = dto.Name,
                        Description = dto.Description,
                        IsEnabledByDefault = dto.IsEnabledByDefault,
                    };

                    AddLibraryCriteria(entity, dto.Criteria);
                    _unitOfWork.Context.TreatmentRuleOverride.Add(entity);
                }
            }

            _unitOfWork.Context.SaveChanges();
        }

        public void DeleteRuleOverride(Guid overrideId)
        {
            var entity = _unitOfWork.Context.TreatmentRuleOverride
                .FirstOrDefault(o => o.Id == overrideId);

            if (entity != null)
            {
                _unitOfWork.Context.TreatmentRuleOverride.Remove(entity);
                _unitOfWork.Context.SaveChanges();
            }
        }

        public List<ScenarioTreatmentRuleOverrideSettingDTO> GetOverrideSettings(Guid simulationId)
        {
            return _unitOfWork.Context.ScenarioTreatmentRuleOverride
                .AsNoTracking()
                .Where(s => s.SimulationId == simulationId)
                .Select(s => new ScenarioTreatmentRuleOverrideSettingDTO
                {
                    OverrideId = s.Id,
                    OverrideName = s.Name,
                    IsEnabled = s.IsEnabled
                })
                .ToList();
        }

        public void UpsertOverrideSettings(Guid simulationId, List<ScenarioTreatmentRuleOverrideSettingDTO> settings)
        {
            settings ??= new List<ScenarioTreatmentRuleOverrideSettingDTO>();

            var existingOverrides = _unitOfWork.Context.ScenarioTreatmentRuleOverride
                .Where(s => s.SimulationId == simulationId)
                .ToList();

            foreach (var setting in settings)
            {
                var existing = existingOverrides.FirstOrDefault(o => o.Id == setting.OverrideId)
                    ?? existingOverrides.FirstOrDefault(o => string.Equals(o.Name, setting.OverrideName, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    existing.IsEnabled = setting.IsEnabled;
                }
            }

            _unitOfWork.Context.SaveChanges();
        }

        public void ReplaceScenarioRuleOverridesFromLibrary(Guid simulationId, Guid libraryId, List<TreatmentDTO> scenarioTreatments)
        {
            scenarioTreatments ??= new List<TreatmentDTO>();

            DeleteScenarioRuleOverrides(simulationId);

            var libraryOverrides = _unitOfWork.Context.TreatmentRuleOverride
                .AsNoTracking()
                .AsSplitQuery()
                .Where(o => o.TreatmentLibraryId == libraryId)
                .Include(o => o.Criteria)
                    .ThenInclude(c => c.TreatmentJoins)
                    .ThenInclude(j => j.SelectableTreatment)
                .OrderBy(o => o.Name)
                .ToList();

            if (!libraryOverrides.Any())
            {
                return;
            }

            var treatmentIdMap = scenarioTreatments
                .Where(t => t.SourceTreatmentId.HasValue)
                .GroupBy(t => t.SourceTreatmentId.Value)
                .ToDictionary(g => g.Key, g => g.First().Id);

            var scenarioTreatmentsByName = scenarioTreatments
                .Where(t => !string.IsNullOrWhiteSpace(t.Name))
                .GroupBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

            var scenarioOverrides = new List<ScenarioTreatmentRuleOverrideEntity>();
            foreach (var libraryOverride in libraryOverrides)
            {
                var scenarioOverride = new ScenarioTreatmentRuleOverrideEntity
                {
                    Id = Guid.NewGuid(),
                    SimulationId = simulationId,
                    SourceTreatmentRuleOverrideId = libraryOverride.Id,
                    Name = libraryOverride.Name,
                    Description = libraryOverride.Description,
                    IsEnabled = libraryOverride.IsEnabledByDefault
                };

                foreach (var criterion in libraryOverride.Criteria.OrderBy(c => c.SortOrder))
                {
                    var scenarioTreatmentIds = criterion.TreatmentJoins
                        .Select(join => ResolveScenarioTreatmentId(join, treatmentIdMap, scenarioTreatmentsByName))
                        .Where(id => id != Guid.Empty)
                        .Distinct()
                        .ToList();

                    if (!scenarioTreatmentIds.Any())
                    {
                        continue;
                    }

                    var scenarioCriterion = new ScenarioTreatmentRuleOverrideCriterionEntity
                    {
                        Id = Guid.NewGuid(),
                        ScenarioTreatmentRuleOverrideId = scenarioOverride.Id,
                        LogicalOperator = NormalizeLogicalOperator(criterion.LogicalOperator),
                        SortOrder = criterion.SortOrder,
                        CriterionExpression = criterion.CriterionExpression
                    };

                    foreach (var scenarioTreatmentId in scenarioTreatmentIds)
                    {
                        scenarioCriterion.TreatmentJoins.Add(new ScenarioTreatmentRuleOverrideCriterionTreatmentEntity
                        {
                            ScenarioTreatmentRuleOverrideCriterionId = scenarioCriterion.Id,
                            ScenarioSelectableTreatmentId = scenarioTreatmentId
                        });
                    }

                    scenarioOverride.Criteria.Add(scenarioCriterion);
                }

                scenarioOverrides.Add(scenarioOverride);
            }

            _unitOfWork.Context.ScenarioTreatmentRuleOverride.AddRange(scenarioOverrides);
            _unitOfWork.Context.SaveChanges();
        }

        public void DeleteScenarioRuleOverrides(Guid simulationId)
        {
            var criterionIds = _unitOfWork.Context.ScenarioTreatmentRuleOverrideCriterion
                .AsNoTracking()
                .Where(criterion => criterion.ScenarioTreatmentRuleOverride.SimulationId == simulationId)
                .Select(criterion => criterion.Id)
                .ToList();

            if (criterionIds.Any())
            {
                _unitOfWork.Context.DeleteAll<ScenarioTreatmentRuleOverrideCriterionTreatmentEntity>(
                    join => criterionIds.Contains(join.ScenarioTreatmentRuleOverrideCriterionId));
            }

            _unitOfWork.Context.DeleteAll<ScenarioTreatmentRuleOverrideCriterionEntity>(
                criterion => criterion.ScenarioTreatmentRuleOverride.SimulationId == simulationId);

            _unitOfWork.Context.DeleteAll<ScenarioTreatmentRuleOverrideEntity>(
                ruleOverride => ruleOverride.SimulationId == simulationId);
        }

        public List<ScenarioTreatmentRuleOverrideCriterionEntity> GetActiveOverrideCriteria(Guid simulationId)
        {
            var overrides = _unitOfWork.Context.ScenarioTreatmentRuleOverride
                .AsNoTracking()
                .AsSplitQuery()
                .Where(o => o.SimulationId == simulationId && o.IsEnabled)
                .Include(o => o.Criteria)
                    .ThenInclude(c => c.TreatmentJoins)
                .ToList();

            return overrides
                .OrderBy(o => o.Name)
                .SelectMany(o => o.Criteria.OrderBy(c => c.SortOrder))
                .Where(c => !string.IsNullOrWhiteSpace(c.CriterionExpression) && c.TreatmentJoins.Any())
                .ToList();
        }

        public void DeleteCriteriaWithoutTreatmentsForLibrary(Guid libraryId)
        {

            _unitOfWork.Context.SaveChanges();
            _unitOfWork.Context.ChangeTracker.Clear();
            var criteriaWithoutTreatments = _unitOfWork.Context.TreatmentRuleOverrideCriterion
                .Where(c => c.TreatmentRuleOverride.TreatmentLibraryId == libraryId && !c.TreatmentJoins.Any())
                .ToList();

            if (criteriaWithoutTreatments.Any())
            {
                _unitOfWork.Context.TreatmentRuleOverrideCriterion.RemoveRange(criteriaWithoutTreatments);
                _unitOfWork.Context.SaveChanges();
            }
        }

        public void DeleteCriteriaWithoutTreatmentsForScenario(Guid simulationId)
        {
            _unitOfWork.Context.SaveChanges();
            _unitOfWork.Context.ChangeTracker.Clear();
            var criteriaWithoutTreatments = _unitOfWork.Context.ScenarioTreatmentRuleOverrideCriterion
                .Where(c => c.ScenarioTreatmentRuleOverride.SimulationId == simulationId && !c.TreatmentJoins.Any())
                .ToList();

            if (criteriaWithoutTreatments.Any())
            {
                _unitOfWork.Context.ScenarioTreatmentRuleOverrideCriterion.RemoveRange(criteriaWithoutTreatments);
                _unitOfWork.Context.SaveChanges();
            }
        }

        private static Guid ResolveScenarioTreatmentId(
            TreatmentRuleOverrideCriterionTreatmentEntity join,
            Dictionary<Guid, Guid> treatmentIdMap,
            Dictionary<string, Guid> scenarioTreatmentsByName)
        {
            if (treatmentIdMap.TryGetValue(join.SelectableTreatmentId, out var scenarioTreatmentId))
            {
                return scenarioTreatmentId;
            }

            return join.SelectableTreatment != null
                && scenarioTreatmentsByName.TryGetValue(join.SelectableTreatment.Name, out var fallbackScenarioTreatmentId)
                    ? fallbackScenarioTreatmentId
                    : Guid.Empty;
        }

        private void ValidateLibraryRuleOverrides(Guid libraryId, List<TreatmentRuleOverrideDTO> overrides)
        {
            var validTreatmentIds = _unitOfWork.Context.SelectableTreatment
                .AsNoTracking()
                .Where(t => t.TreatmentLibraryId == libraryId)
                .Select(t => t.Id)
                .ToHashSet();

            foreach (var ruleOverride in overrides)
            {
                foreach (var criterion in ruleOverride.Criteria ?? new List<TreatmentRuleOverrideCriterionDTO>())
                {
                    NormalizeLogicalOperator(criterion.LogicalOperator);

                    if (string.IsNullOrWhiteSpace(criterion.CriterionExpression))
                    {
                        throw new InvalidOperationException("Each treatment override criterion must have a criterion expression.");
                    }

                    var treatmentIds = GetTreatmentIds(criterion);
                    if (!treatmentIds.Any())
                    {
                        throw new InvalidOperationException("Each treatment override criterion must select at least one treatment.");
                    }

                    if (treatmentIds.Any(id => !validTreatmentIds.Contains(id)))
                    {
                        throw new InvalidOperationException("Treatment override criteria can only reference treatments in the same treatment library.");
                    }
                }
            }
        }

        private void ReplaceLibraryCriteria(TreatmentRuleOverrideEntity existing, List<TreatmentRuleOverrideCriterionDTO> criteria)
        {
            var existingCriteria = existing.Criteria.ToList();
            foreach (var criterion in existingCriteria)
            {
                _unitOfWork.Context.TreatmentRuleOverrideCriterionTreatment.RemoveRange(criterion.TreatmentJoins);
            }

            _unitOfWork.Context.TreatmentRuleOverrideCriterion.RemoveRange(existingCriteria);
            existing.Criteria.Clear();
            AddLibraryCriteria(existing, criteria);
        }

        private static void AddLibraryCriteria(TreatmentRuleOverrideEntity entity, List<TreatmentRuleOverrideCriterionDTO> criteria)
        {
            var index = 0;
            foreach (var criterionDto in criteria ?? new List<TreatmentRuleOverrideCriterionDTO>())
            {
                var criterionEntity = new TreatmentRuleOverrideCriterionEntity
                {
                    Id = criterionDto.Id != Guid.Empty ? criterionDto.Id : Guid.NewGuid(),
                    TreatmentRuleOverrideId = entity.Id,
                    LogicalOperator = NormalizeLogicalOperator(criterionDto.LogicalOperator),
                    SortOrder = index++,
                    CriterionExpression = criterionDto.CriterionExpression
                };

                foreach (var treatmentId in GetTreatmentIds(criterionDto).Distinct())
                {
                    criterionEntity.TreatmentJoins.Add(new TreatmentRuleOverrideCriterionTreatmentEntity
                    {
                        TreatmentRuleOverrideCriterionId = criterionEntity.Id,
                        SelectableTreatmentId = treatmentId
                    });
                }

                entity.Criteria.Add(criterionEntity);
            }
        }

        private static List<Guid> GetTreatmentIds(TreatmentRuleOverrideCriterionDTO criterion) =>
            criterion.TreatmentIds != null && criterion.TreatmentIds.Any()
                ? criterion.TreatmentIds.Where(id => id != Guid.Empty).ToList()
                : criterion.Treatments?
                    .Where(t => t.Id != Guid.Empty)
                    .Select(t => t.Id)
                    .ToList() ?? new List<Guid>();

        private static string NormalizeLogicalOperator(string logicalOperator)
        {
            if (string.Equals(logicalOperator, OrOperator, StringComparison.OrdinalIgnoreCase))
            {
                return OrOperator;
            }

            if (string.Equals(logicalOperator, AndOperator, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(logicalOperator))
            {
                return AndOperator;
            }

            throw new InvalidOperationException("Treatment override criteria only support AND or OR append operators.");
        }
    }
}
