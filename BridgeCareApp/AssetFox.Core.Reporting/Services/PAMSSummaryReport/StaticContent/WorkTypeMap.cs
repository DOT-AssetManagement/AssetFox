
using System.Collections.Generic;
using AssetFox.Core.DTOs.Enums;

namespace AssetFox.Core.Reporting.Services.PAMSSummaryReport.StaticContent
{
    public static class WorkTypeMap
    {
        public static Dictionary<string, TreatmentCategory> Map =
            new()
            {
                // MPMS
                { "Preservation", TreatmentCategory.Preservation },
                { "Capacity Adding", TreatmentCategory.CapacityAdding },
                { "Rehabilitation", TreatmentCategory.Rehabilitation },
                { "Replacement", TreatmentCategory.Reconstruction },
                { "Reconstruction", TreatmentCategory.Reconstruction },
                { "Maintenance", TreatmentCategory.Maintenance },
                { "Other", TreatmentCategory.Other },
            };
    }
}
