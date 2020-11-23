namespace BridgeCare.Models.SummaryReport
{
    public class WorkSummaryByBudgetModel
    {
        public int YEARS { get; set; }

        public string TREATMENT { get; set; }

        public string BUDGET { get; set; }

        public double CostPerTreatmentPerYear { get; set; }
    }
}
