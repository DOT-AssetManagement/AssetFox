using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Analysis.Engine;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class TreatmentOptionDetails
    {
        public static TreatmentOptionDetail Detail()
        {
            var detail = new TreatmentOptionDetail(
                "Treatment", 100, 200, 3, 9);
            return detail;
        }
    }
}
