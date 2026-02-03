using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.Tests;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class AssetSummaryDetails
    {
        public static AssetSummaryDetail Detail(string assetName, Guid assetId, List<string> numericAttributeNames, List<string> textAttributeNames)
        {
            var detail = new AssetSummaryDetail(assetName, assetId);
            foreach (var textAttributeName in textAttributeNames)
            {
                detail.ValuePerTextAttribute[textAttributeName] = TestAttributeValues.GenericStringValue;
            }
            foreach (var numericAttributeName in numericAttributeNames)
            {
                detail.ValuePerNumericAttribute[numericAttributeName] = 6;
            }
            return detail;
        }

        public static AssetSummaryDetail Detail(SimulationOutputSetupContext setupContext, AssetNameIdPair assetNameIdPair)
            => Detail(assetNameIdPair.Name, assetNameIdPair.Id, setupContext.NumericAttributeNames, setupContext.TextAttributeNames);
    }
}
