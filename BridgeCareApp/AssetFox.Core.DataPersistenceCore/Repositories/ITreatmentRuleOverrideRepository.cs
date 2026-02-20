using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface ITreatmentRuleOverrideRepository
    {
        List<TreatmentRuleOverrideDTO> GetRuleOverridesByLibraryId(Guid libraryId);

        void UpsertRuleOverrides(Guid libraryId, List<TreatmentRuleOverrideDTO> overrides);

        void DeleteRuleOverride(Guid overrideId);

        List<ScenarioTreatmentRuleOverrideSettingDTO> GetOverrideSettings(Guid simulationId);

        void UpsertOverrideSettings(Guid simulationId, List<ScenarioTreatmentRuleOverrideSettingDTO> settings);
    }
}
