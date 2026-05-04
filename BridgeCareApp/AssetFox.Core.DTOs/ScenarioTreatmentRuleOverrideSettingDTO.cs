using System;

namespace AssetFox.Core.DTOs
{
    public class ScenarioTreatmentRuleOverrideSettingDTO
    {
        public Guid OverrideId { get; set; }

        public string OverrideName { get; set; }

        public bool IsEnabled { get; set; }
    }
}
