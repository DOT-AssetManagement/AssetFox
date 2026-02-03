using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Analysis.Engine;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class TreatmentRejectionDetails
    {
        public static TreatmentRejectionDetail Detail()
        {
            var detail = new TreatmentRejectionDetail(
                "Treatment",
                TreatmentRejectionReason.Undefined,
                default);
            return detail;
        }
    }
}
