using System.Collections.Generic;
using AssetFox.Core.DTOs.Enums;

namespace AssetFox.Core.Reporting.Models.BAMSSummaryReport
{
    public class WorkTypeTotalAggregated
    {
        public Dictionary<TreatmentCategory, SortedDictionary<int, decimal>> WorkTypeTotalCulvert;
        public Dictionary<TreatmentCategory, SortedDictionary<int, decimal>> WorkTypeTotalBridge;
        public Dictionary<TreatmentCategory, SortedDictionary<int, decimal>> WorkTypeTotalMPMS;
        public Dictionary<TreatmentCategory, SortedDictionary<int, decimal>> WorkTypeTotalSAP;
        public Dictionary<TreatmentCategory, SortedDictionary<int, decimal>> WorkTypeTotalProjectBuilder;
        public Dictionary<TreatmentCategory, SortedDictionary<int, decimal>> WorkTypeTotalWorkOutsideScope;
        public Dictionary<TreatmentCategory, SortedDictionary<int, decimal>> WorkTypeTotalBundled;
        public Dictionary<TreatmentCategory, SortedDictionary<int, decimal>> WorkTypeTotalCommitted;        
    }
}
