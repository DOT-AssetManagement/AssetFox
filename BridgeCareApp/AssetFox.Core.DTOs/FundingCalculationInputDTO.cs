using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class FundingCalculationInputDTO : BaseDTO
    {
        public IList<BudgetToSpendDTO> CurrentBudgetsToSpend { get; set; } = new List<BudgetToSpendDTO>();        
    }
}
