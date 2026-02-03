using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.Treatment
{
    public class TreatmentCostLoadResult
    {
        public List<TreatmentCostDTO> Costs { get; set; }
        public List<string> ValidationMessages { get; set; }
    }
}
