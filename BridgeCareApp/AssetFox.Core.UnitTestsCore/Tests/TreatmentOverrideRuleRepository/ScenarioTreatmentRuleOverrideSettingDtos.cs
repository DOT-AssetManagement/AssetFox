using System;
using System.Collections.Generic;
using System.Text;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests.TreatmentOverrideRuleRepository;

public static class ScenarioTreatmentRuleOverrideSettingDtos
{
    public static ScenarioTreatmentRuleOverrideSettingDTO New(Guid overrideId, string overrideName, bool isEnabled)
    {
        return new ScenarioTreatmentRuleOverrideSettingDTO
        {
            OverrideId = overrideId,
            OverrideName = overrideName,
            IsEnabled = isEnabled,
        };
    }
}
