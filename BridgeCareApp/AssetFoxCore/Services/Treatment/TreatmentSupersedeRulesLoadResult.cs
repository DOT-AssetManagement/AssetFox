using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.Treatment
{
    public class TreatmentSupersedeRulesLoadResult
    {        
        public Dictionary<Guid, List<TreatmentSupersedeRuleDTO>> supersedeRulesPerTreatmentIdDict { get; set; }

        public List<string> ValidationMessages { get; set; }
    }
}
