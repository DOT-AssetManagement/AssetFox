using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Treatment;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL
{
    public class TreatmentRuleOverrideRepository : ITreatmentRuleOverrideRepository
    {
        private readonly UnitOfDataPersistenceWork _unitOfWork;

        public TreatmentRuleOverrideRepository(UnitOfDataPersistenceWork unitOfWork) =>
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

        public List<TreatmentRuleOverrideDTO> GetRuleOverridesByLibraryId(Guid libraryId)
        {
            var entities = _unitOfWork.Context.TreatmentRuleOverride
                .AsNoTracking()
                .Where(o => o.TreatmentLibraryId == libraryId)
                .Include(o => o.Criteria)
                .OrderBy(o => o.Name)
                .ToList();

            return entities.Select(e => e.ToDto()).ToList();
        }

        public void UpsertRuleOverrides(Guid libraryId, List<TreatmentRuleOverrideDTO> overrides)
        {
            var existingOverrides = _unitOfWork.Context.TreatmentRuleOverride
                .Where(o => o.TreatmentLibraryId == libraryId)
                .Include(o => o.Criteria)
                .ToList();

            // Delete overrides that are no longer in the DTO list
            var incomingIds = overrides.Select(o => o.Id).ToHashSet();
            var toDelete = existingOverrides.Where(e => !incomingIds.Contains(e.Id)).ToList();
            _unitOfWork.Context.TreatmentRuleOverride.RemoveRange(toDelete);

            foreach (var dto in overrides)
            {
                var existing = existingOverrides.FirstOrDefault(e => e.Id == dto.Id);
                if (existing != null)
                {
                    // Update existing override
                    existing.Name = dto.Name;
                    existing.Description = dto.Description;
                    existing.IsEnabledByDefault = dto.IsEnabledByDefault;

                    // Replace criteria: remove old, add new
                    _unitOfWork.Context.TreatmentRuleOverrideCriterion.RemoveRange(existing.Criteria);
                    foreach (var criterionDto in dto.Criteria)
                    {
                        existing.Criteria.Add(new TreatmentRuleOverrideCriterionEntity
                        {
                            Id = criterionDto.Id != Guid.Empty ? criterionDto.Id : Guid.NewGuid(),
                            TreatmentRuleOverrideId = existing.Id,
                            TreatmentName = criterionDto.TreatmentName,
                            CriterionExpression = criterionDto.CriterionExpression
                        });
                    }
                }
                else
                {
                    // Add new override
                    var entity = new TreatmentRuleOverrideEntity
                    {
                        Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
                        TreatmentLibraryId = libraryId,
                        Name = dto.Name,
                        Description = dto.Description,
                        IsEnabledByDefault = dto.IsEnabledByDefault,
                    };

                    foreach (var criterionDto in dto.Criteria)
                    {
                        entity.Criteria.Add(new TreatmentRuleOverrideCriterionEntity
                        {
                            Id = criterionDto.Id != Guid.Empty ? criterionDto.Id : Guid.NewGuid(),
                            TreatmentName = criterionDto.TreatmentName,
                            CriterionExpression = criterionDto.CriterionExpression
                        });
                    }

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
            return _unitOfWork.Context.ScenarioTreatmentRuleOverrideSetting
                .AsNoTracking()
                .Where(s => s.SimulationId == simulationId)
                .Select(s => new ScenarioTreatmentRuleOverrideSettingDTO
                {
                    OverrideName = s.OverrideName,
                    IsEnabled = s.IsEnabled
                })
                .ToList();
        }

        public void UpsertOverrideSettings(Guid simulationId, List<ScenarioTreatmentRuleOverrideSettingDTO> settings)
        {
            var existingSettings = _unitOfWork.Context.ScenarioTreatmentRuleOverrideSetting
                .Where(s => s.SimulationId == simulationId)
                .ToList();

            // Remove all existing settings for this simulation
            _unitOfWork.Context.ScenarioTreatmentRuleOverrideSetting.RemoveRange(existingSettings);

            // Add the new settings
            foreach (var setting in settings)
            {
                _unitOfWork.Context.ScenarioTreatmentRuleOverrideSetting.Add(
                    new ScenarioTreatmentRuleOverrideSettingEntity
                    {
                        Id = Guid.NewGuid(),
                        SimulationId = simulationId,
                        OverrideName = setting.OverrideName,
                        IsEnabled = setting.IsEnabled
                    });
            }

            _unitOfWork.Context.SaveChanges();
        }

        /// <summary>
        /// Gets the list of active override criteria for a simulation, resolving
        /// scenario-level toggle settings against the library's override definitions.
        /// </summary>
        public List<TreatmentRuleOverrideCriterionEntity> GetActiveOverrideCriteria(Guid simulationId, Guid libraryId)
        {
            var overrides = _unitOfWork.Context.TreatmentRuleOverride
                .AsNoTracking()
                .Where(o => o.TreatmentLibraryId == libraryId)
                .Include(o => o.Criteria)
                .ToList();

            if (!overrides.Any())
            {
                return new List<TreatmentRuleOverrideCriterionEntity>();
            }

            var scenarioSettings = _unitOfWork.Context.ScenarioTreatmentRuleOverrideSetting
                .AsNoTracking()
                .Where(s => s.SimulationId == simulationId)
                .ToDictionary(s => s.OverrideName, s => s.IsEnabled);

            var activeCriteria = new List<TreatmentRuleOverrideCriterionEntity>();

            foreach (var ruleOverride in overrides)
            {
                // Use scenario setting if it exists, otherwise fall back to default
                var isEnabled = scenarioSettings.TryGetValue(ruleOverride.Name, out var setting)
                    ? setting
                    : ruleOverride.IsEnabledByDefault;

                if (isEnabled)
                {
                    activeCriteria.AddRange(ruleOverride.Criteria);
                }
            }

            return activeCriteria;
        }
    }
}
