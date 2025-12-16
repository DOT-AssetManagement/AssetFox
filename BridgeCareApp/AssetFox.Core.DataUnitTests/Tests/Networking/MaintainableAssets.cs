using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Data;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.TestHelpers;

namespace AssetFox.Core.DataUnitTests.Tests
{
    public static class MaintainableAssets
    {
        public static MaintainableAsset InNetwork(Guid networkId, string keyAttributeName, Guid? assetId = null, SectionLocation? location = null)
        {
            var resolveAssetId = assetId ?? Guid.NewGuid();
            SectionLocation resolveLocation;
            if (location == null)
            {
                var locationName = RandomStrings.WithPrefix("location");
                resolveLocation = Locations.Section(locationName);
            } else
            {
                resolveLocation = location;
            }
            var maintainableAsset = new MaintainableAsset(resolveAssetId, networkId, resolveLocation, keyAttributeName);
            return maintainableAsset;
        }
    }
}
