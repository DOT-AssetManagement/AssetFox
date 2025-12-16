using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class TreatmentConsiderationDetailDTO : BaseDTO
    {
        public int? BudgetPriorityLevel { get; set; }        

        public IList<CashFlowConsiderationDetailDTO> CashFlowConsiderations { get; set; } = new List<CashFlowConsiderationDetailDTO>();

        public string TreatmentName { get; set; }

        public FundingCalculationInputDTO FundingCalculationInput { get; set; }

        public FundingCalculationOutputDTO FundingCalculationOutput { get; set; }
    }
}
