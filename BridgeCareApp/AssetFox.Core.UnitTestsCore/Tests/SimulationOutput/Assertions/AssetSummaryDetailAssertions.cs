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
    public static class AssetSummaryDetailAssertions
    {
        public static void Same(AssetSummaryDetail expected, AssetSummaryDetail actual)
        {
            Assert.Equal(expected.AssetName, actual.AssetName);
            DoubleDictionaryAssertions.ApproximatelySame(expected.ValuePerNumericAttribute, actual.ValuePerNumericAttribute);
            DictionaryAssertions.Same(expected.ValuePerTextAttribute, actual.ValuePerTextAttribute);
        }
    }
}
