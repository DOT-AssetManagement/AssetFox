using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class TreatmentSupersedeRuleImportResultDTO : WarningServiceResultDTO
    {
        public Dictionary<Guid, List<TreatmentSupersedeRuleDTO>> supersedeRulesPerTreatmentIdDict { get; set; } = new Dictionary<Guid, List<TreatmentSupersedeRuleDTO>>();
    }
}
