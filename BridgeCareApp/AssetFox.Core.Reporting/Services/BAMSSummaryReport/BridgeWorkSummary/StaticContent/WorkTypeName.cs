using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs.Enums;

namespace AssetFox.Core.Reporting.Services.BAMSSummaryReport.BridgeWorkSummary
{
    public static class WorkTypeNamesExtensions
    {
        public static string ToSpreadsheetString(this TreatmentCategory name) => name switch
        {
            TreatmentCategory.Preservation => "Preservation",
            TreatmentCategory.CapacityAdding => "Capacity Adding",
            TreatmentCategory.Rehabilitation => "Rehabilitation",            
            TreatmentCategory.Replacement => "Replacement",
            TreatmentCategory.Maintenance => "Maintenance",
            TreatmentCategory.Other => "Other",
            TreatmentCategory.WorkOutsideScope => "Work Outside Scope/Jurisdiction",
            _ => name.ToString(),
        };
    }
}
