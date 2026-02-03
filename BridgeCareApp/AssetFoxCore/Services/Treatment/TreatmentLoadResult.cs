using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.Treatment
{
    public class TreatmentLoadResult
    {
        public TreatmentDTO Treatment { get; set; }
        public List<string> ValidationMessages { get; set; }
    }
}
