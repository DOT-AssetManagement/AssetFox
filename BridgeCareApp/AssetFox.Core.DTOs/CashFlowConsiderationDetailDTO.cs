using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class CashFlowConsiderationDetailDTO : BaseDTO
    {
        public string CashFlowRuleName { get; set; }

        public int ReasonAgainstCashFlow { get; set; }
    }
}
