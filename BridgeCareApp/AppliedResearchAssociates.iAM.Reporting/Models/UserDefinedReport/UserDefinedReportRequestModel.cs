using System.Collections.Generic;

namespace AppliedResearchAssociates.iAM.Reporting.Models
{
    public class UserDefinedReportRequestModel
    {
        // Filters

        public List<string> Attributes { get; set; } // TODO InitialAssetSummaries' AssetSummaryDetailValuesIntId details with given Attributes

        public List<int> Years { get; set; }

        // TODO Show/Hide (next level report)
        public bool DisplayBudgets { get; set; } = true;
                
        public bool DisplayDeficientConditionGoals { get; set; } = true;        
                
        public bool DisplayTargetConditionGoals { get; set; } = true;
                
        public bool DisplayAssets { get; set; } = true;
        // TODO Assets => further drill down for show/hide can be planned as required...
    }
}
