using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class TreatmentRejectionDetailDTO : BaseDTO
    {
        public double PotentialConditionChange { get; set; }

        public string TreatmentName { get; set; }

        public int TreatmentRejectionReason { get; set; }
    }
}
