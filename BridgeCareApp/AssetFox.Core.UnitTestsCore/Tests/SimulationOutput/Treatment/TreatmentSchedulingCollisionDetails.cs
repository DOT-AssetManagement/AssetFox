using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Analysis.Engine;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class TreatmentSchedulingCollisionDetails
    {
        public static TreatmentSchedulingCollisionDetail Detail(int year)
        {
            var detail = new TreatmentSchedulingCollisionDetail(year, "Treatment");
            return detail;
        }
    }
}
