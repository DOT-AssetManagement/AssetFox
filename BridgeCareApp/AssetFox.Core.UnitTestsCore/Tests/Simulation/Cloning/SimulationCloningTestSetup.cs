using System;
using System.Collections.Generic;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.DataUnitTests;
using AssetFox.Core.UnitTestsCore.Tests.Attributes;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.TestUtils;

namespace AssetFox.Core.UnitTestsCore.Tests.SimulationCloning
{
    public static class SimulationCloningTestSetup
    {
        public static Guid TestNetworkIdInDatabase()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var networkId = NetworkTestSetup.NetworkId;
            return networkId;
        }

        public static Guid TestDestinationNetworkIdInDatabase()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var maintainableAssets = new List<MaintainableAsset>();
            var destinationNetworkId = Guid.NewGuid();
            NetworkTestSetup.ModelForEntityInDbWithExistingKeyAttribute(TestHelper.UnitOfWork, maintainableAssets, TestAttributeIds.BrKeyId, destinationNetworkId);
            return destinationNetworkId;
        }

    }
}
