using System.Collections.Generic;

namespace AssetFox.Core.Reporting.Models.BAMSSummaryReport
{
    public class ProjectRowNumberModel
    {
        public Dictionary<string, int> TreatmentsCount { get; set; } = new Dictionary<string, int>();
    }
}
