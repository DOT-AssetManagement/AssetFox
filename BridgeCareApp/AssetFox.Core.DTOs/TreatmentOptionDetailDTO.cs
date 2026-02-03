using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class TreatmentOptionDetailDTO : BaseDTO
    {
        public double Benefit { get; set; }

        public double ConditionChange { get; set; }

        public double Cost { get; set; }

        public double? RemainingLife { get; set; }

        public string TreatmentName { get; set; }
    }
}
