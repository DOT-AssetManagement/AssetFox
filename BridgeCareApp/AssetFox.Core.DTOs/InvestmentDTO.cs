using System.Collections.Generic;

namespace AssetFox.Core.DTOs
{
    public class InvestmentDTO
    {
        public InvestmentPlanDTO InvestmentPlan { get; set; }

        public List<BudgetDTO> ScenarioBudgets { get; set; }
    }
}
