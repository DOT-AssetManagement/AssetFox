using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.Data;
using AssetFox.Core.Data.Attributes;
using AssetFox.Core.Data.Mappers;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.DataUnitTests.Tests;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.TestUtils;
using Xunit;

namespace AssetFox.Core.UnitTestsCore.Tests.AttributeDatum
{
    public class AttributeDatumRepositoryTests
    {
        [Fact]
        public void GetAllInNetwork_AssetInNetworkWithAttributeDatum_Gets()
        {
            var networkId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var keyAttributeId = Guid.NewGuid();
            var keyAttributeName = RandomStrings.WithPrefix("KeyAttribute");
            var attributeDto = AttributeTestSetup.CreateSingleTextAttribute(TestHelper.UnitOfWork, keyAttributeId, keyAttributeName, ConnectionType.EXCEL, "location");
            var asset = MaintainableAssets.InNetwork(networkId, keyAttributeName, assetId);
            var assets = new List<MaintainableAsset> { asset };
            var network = NetworkTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, assets, networkId, keyAttributeId);
            var assetIds = new List<Guid> { assetId };
            var attributeIds = new List<Guid> { keyAttributeId };
            var datumId = AttributeDatumTestSetup.AssignStringAttributeDatum(attributeDto, asset);
            TestHelper.UnitOfWork.Context.ChangeTracker.Clear();

            var attributeData = TestHelper.UnitOfWork.AttributeDatumRepo.GetAllInNetwork(
                assetIds, attributeIds);

            var attributeDatum = attributeData.Single();
            var expectedAttributeDatum = new AttributeDatumDTO
            {
                Id = datumId,
                MaintainableAssetId = assetId,
                TextValue = "where",
                Attribute = keyAttributeName,
            };
            ObjectAssertions.Equivalent(expectedAttributeDatum, attributeDatum);
        }


    }
}
