using System.Collections.Generic;

namespace AssetFox.Core.Reporting.Models.BAMSSummaryReport
{
    public class WorkSummaryByBudgetModel
    {
        public string Budget { get; set; }

        public List<YearsData> YearlyData { get; set; }
    }
}
