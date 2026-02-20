using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class TreatmentRuleOverrideDTO : BaseDTO
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsEnabledByDefault { get; set; } = true;

        public List<TreatmentRuleOverrideCriterionDTO> Criteria { get; set; } = new List<TreatmentRuleOverrideCriterionDTO>();
    }
}
