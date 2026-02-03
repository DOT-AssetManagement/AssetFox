using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.Treatment
{
    public class TreatmentConsequenceLoadResult
    {
        public List<TreatmentConsequenceDTO> Consequences { get; set; }
        public List<string> ValidationMessages { get; set; }
    }
}
