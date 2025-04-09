using System;
using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class TreatmentOptionDetailDTO : BaseDTO
    {
        public Guid AssetDetailId { get; set; }

        public double Benefit { get; set; }

        public double ConditionChange { get; set; }

        public double Cost { get; set; }

        public double? RemainingLife { get; set; }

        public string TreatmentName { get; set; }
    }
}
