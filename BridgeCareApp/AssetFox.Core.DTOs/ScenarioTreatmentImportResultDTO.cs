using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class ScenarioTreatmentImportResultDTO : WarningServiceResultDTO
    {        
        public List<TreatmentDTO> Treatments { get; set; }
    }
}
