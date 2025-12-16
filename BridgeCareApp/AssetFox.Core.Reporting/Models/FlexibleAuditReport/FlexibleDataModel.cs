using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Analysis.Engine;

namespace AssetFox.Core.Reporting.Models.FlexibleAuditReport
{
    public class FlexibleDataModel
    {
        public double CRS { get; set; }

        public AssetSummaryDetail AssetSummaryDetail { get; set; }
    }
}
