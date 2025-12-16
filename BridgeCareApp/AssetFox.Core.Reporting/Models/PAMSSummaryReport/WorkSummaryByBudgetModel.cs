using System.Collections.Generic;

namespace AssetFox.Core.Reporting.Models.PAMSSummaryReport
{
    public class WorkSummaryByBudgetModel
    {
        public string BudgetName { get; set; }

        public List<YearsData> YearlyData { get; set; }
    }
}
