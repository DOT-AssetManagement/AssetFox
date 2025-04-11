using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class CashFlowConsiderationDetailDTO : BaseDTO
    {
        public string CashFlowRuleName { get; set; }

        public int ReasonAgainstCashFlow { get; set; }
    }
}
