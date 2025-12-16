using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.Treatment
{
    public class TreatmentPerformanceFactorLoadResult
    {
        public List<TreatmentPerformanceFactorDTO> PerformanceFactors { get; set; }
        public List<string> ValidationMessages { get; set; }
    }
}

