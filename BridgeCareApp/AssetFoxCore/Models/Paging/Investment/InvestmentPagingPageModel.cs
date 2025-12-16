using AssetFox.Core.DTOs;

namespace AssetFoxCore.Models
{
    public class InvestmentPagingPageModel : PagingPageModel<BudgetDTO>
    {
        public InvestmentPlanDTO InvestmentPlan { get; set; }
        public int LastYear { get; set; }
        public int FirstYear { get; set; }
    }
}
