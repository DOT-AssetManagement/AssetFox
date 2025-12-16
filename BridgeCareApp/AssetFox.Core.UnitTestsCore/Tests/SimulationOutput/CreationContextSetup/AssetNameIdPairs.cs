using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.TestHelpers;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class AssetNameIdPairs
    {
        public static AssetNameIdPair ForAssetSummaryDetail(AssetSummaryDetail a) => new AssetNameIdPair
        {
            Id = a.AssetId,
            Name = a.AssetName,
        };

        public static AssetNameIdPair Random()
        {
            var name = RandomStrings.WithPrefix("AssetName");
            var id = Guid.NewGuid();
            var pair = new AssetNameIdPair
            {
                Id = id,
                Name = name,
            };
            return pair;
        }
    }
}
