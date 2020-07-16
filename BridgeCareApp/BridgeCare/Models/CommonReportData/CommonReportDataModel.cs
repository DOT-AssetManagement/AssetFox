using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BridgeCare.Models.SummaryReport;
using BridgeCare.Models.SummaryReport.ParametersTAB;

namespace BridgeCare.Models.CommonReportData
{
    public class CommonReportDataModel
    {
        public SortedSet<SimulationDataModel> SimulationDataModels { get; set; }

        public List<BudgetsPerBRKey> BudgetsPerBRKeys { get; set; }

        public ParametersModel ParametersModel { get; set; }
        //public List<Section> SectionsForSummaryReport { get; set; }
    }
}
