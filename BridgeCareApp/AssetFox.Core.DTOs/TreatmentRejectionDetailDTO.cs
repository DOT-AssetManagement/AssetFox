using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class TreatmentRejectionDetailDTO : BaseDTO
    {
        public double PotentialConditionChange { get; set; }

        public string TreatmentName { get; set; }

        public int TreatmentRejectionReason { get; set; }
    }
}
