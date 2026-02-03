using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class PerformanceCurveLibraryDTO : BaseLibraryDTO
    {
        public List<PerformanceCurveDTO> PerformanceCurves { get; set; } = new List<PerformanceCurveDTO>();
    }
}
