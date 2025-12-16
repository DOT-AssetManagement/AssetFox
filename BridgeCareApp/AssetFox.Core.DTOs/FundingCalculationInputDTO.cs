using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class FundingCalculationInputDTO : BaseDTO
    {
        public IList<BudgetToSpendDTO> CurrentBudgetsToSpend { get; set; } = new List<BudgetToSpendDTO>();        
    }
}
