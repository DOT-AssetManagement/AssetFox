using System.Collections.Generic;

namespace AppliedResearchAssociates.iAM.Reporting.Models
{
    /// <summary>
    /// Filters for user defined report
    /// </summary>
    public class UserDefinedReportRequestModel
    {
        public List<string> Attributes { get; set; }

        public List<int> Years { get; set; }

        public bool DisplayConditionOfNetwork { get; set; }

        public bool DisplayInitialAssets { get; set; }

        public bool DisplayYearAssets { get; set; }

        public bool DisplayBudgets { get; set; }

        public bool DisplayDeficientConditionGoals { get; set; }

        public bool DisplayTargetConditionGoals { get; set; }

        public bool DisplayTreatmentOptions { get; set; }

        public bool DisplayTreatmentSchedulingCollisions { get; set; }

        public bool DisplayTreatmentRejections { get; set; }

        public bool DisplayTreatmentCashflowConsiderations { get; set; }

        public bool DisplyTreatmentCurrentBudgetsToSpend { get; set; }

        public bool DisplayTreatmentAllocations { get; set; }
    }
}
