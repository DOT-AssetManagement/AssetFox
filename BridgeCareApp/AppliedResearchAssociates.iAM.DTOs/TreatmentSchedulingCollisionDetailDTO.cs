using System;
using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class TreatmentSchedulingCollisionDetailDTO : BaseDTO
    {
        public Guid AssetDetailId { get; set; }

        public string NameOfUnscheduledTreatment { get; set; }
    }
}
