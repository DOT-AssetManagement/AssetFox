using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class TreatmentPerformanceFactorDtos
    {
        public static TreatmentPerformanceFactorDTO Dto(string attribute, Guid? id = null)
        {
            var resolveId = id ?? Guid.NewGuid();
            var dto = new TreatmentPerformanceFactorDTO
            {
                Attribute = attribute,
                Id = resolveId,
                PerformanceFactor = 2f,
            };
            return dto;
        }
    }
}
