using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface ITreatmentRuleOverrideRepository
    {
        List<TreatmentRuleOverrideDTO> GetRuleOverridesByLibraryId(Guid libraryId);

        List<TreatmentRuleOverrideDTO> GetRuleOverridesBySimulationId(Guid simulationId);

        void UpsertRuleOverrides(Guid libraryId, List<TreatmentRuleOverrideDTO> overrides);

        void DeleteRuleOverride(Guid overrideId);

        Guid? GetLibraryIdForRuleOverride(Guid overrideId);

        List<ScenarioTreatmentRuleOverrideSettingDTO> GetOverrideSettings(Guid simulationId);

        void UpsertOverrideSettings(Guid simulationId, List<ScenarioTreatmentRuleOverrideSettingDTO> settings);

        void ReplaceScenarioRuleOverridesFromLibrary(Guid simulationId, Guid libraryId, List<TreatmentDTO> scenarioTreatments);

        void DeleteScenarioRuleOverrides(Guid simulationId);

        // Should we remove this from the interface? It returns entities.
        List<ScenarioTreatmentRuleOverrideCriterionEntity> GetActiveOverrideCriteria(Guid simulationId);

        void DeleteCriteriaWithoutTreatmentsForLibrary(Guid libraryId);

        void DeleteCriteriaWithoutTreatmentsForScenario(Guid simulationId);
    }
}
