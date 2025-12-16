using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class CashFlowRuleLibraryDTO : BaseLibraryDTO
    {
        public List<CashFlowRuleDTO> CashFlowRules { get; set; } = new List<CashFlowRuleDTO>();
    }
}
