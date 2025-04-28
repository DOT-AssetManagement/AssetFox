using System.Collections.Generic;

namespace AppliedResearchAssociates.iAM.Reporting.Models
{
    public class UserDefinedReportRequestModel
    {
        // Filters

        public List<string> Attributes { get; set; } // TODO InitialAssetSummaries' AssetSummaryDetailValuesIntId details with given Attributes

        public List<int> Years { get; set; }

        // TODO Show/Hide (next level report)
        public bool DisplayBudgets { get; set; }
                
        public bool DisplayDeficientConditionGoals { get; set; }
                
        public bool DisplayTargetConditionGoals { get; set; }
                
        public bool DisplayAssets { get; set; }
        // TODO Assets => further drill down for show/hide can be planned as required...
    }
}
