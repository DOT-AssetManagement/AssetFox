using System;
using System.Collections.Generic;
using System.Text;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class TreatmentPerformanceFactorDTO : BaseDTO
    {
        public string Attribute { get; set; }

        public float PerformanceFactor { get; set; }
    }
}
