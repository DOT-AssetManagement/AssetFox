using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{

    public class ScenarioPerformanceCurvesImportResultDTO : WarningServiceResultDTO
    {

        public List<PerformanceCurveDTO> PerformanceCurves { get; set; }
    }
}
