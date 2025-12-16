using System.Collections.Generic;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Enums;

namespace AssetFoxCore.Models
{
    public class CommittedProjectFillTreatmentReturnValuesModel
    {
        public List<CommittedProjectConsequenceDTO> ValidTreatmentConsequences { get; set; }
        public double TreatmentCost { get; set; }
        public TreatmentCategory TreatmentCategory { get; set; }
    }
}
