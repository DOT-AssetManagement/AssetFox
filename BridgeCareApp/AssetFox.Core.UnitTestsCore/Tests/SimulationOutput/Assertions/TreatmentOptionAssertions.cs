using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.TestHelpers;
using Xunit;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class TreatmentOptionAssertions
    {
        public static void Same(TreatmentOptionDetail expected, TreatmentOptionDetail actual)
        {
            Assert.Equal(expected.TreatmentName, actual.TreatmentName);
            DoubleAssertions.ApproximatelyEqual(expected.Benefit, actual.Benefit);
            DoubleAssertions.ApproximatelyEqual(expected.Cost, actual.Cost);
            NullableDoubleAssertions.ApproximatelyEqual(expected.RemainingLife, actual.RemainingLife);
        }
    }
}
