using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class TreatmentRuleOverrideCriterionDTO : BaseDTO
    {
        public string TreatmentName { get; set; }

        public string CriterionExpression { get; set; }
    }
}
