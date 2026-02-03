using Xunit;
using System;
using System.Collections.Generic;
using AssetFox.Core.Data.Attributes;
using AssetFox.Core.Data;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.DataUnitTests.TestUtils;

namespace AssetFox.Core.DataUnitTests.Tests.Networking
{
    public class NetworkFactoryTests
    {
        private readonly Guid sectionLocationId = Guid.Empty;
        private readonly SectionLocation sectionLocation;
        List<IAttributeDatum> attributeData;

        public NetworkFactoryTests()
        {
            sectionLocation = new SectionLocation(sectionLocationId, CommonTestParameterValues.LocationIdentifier1);
            attributeData = new List<IAttributeDatum>();            
        }

        [Fact]
        public void CreateNetworkFromAttributeDataRecords_SectionLocationInDb_CreatesWithMaintainableAsset()
        {
            // Arrange
            attributeData.Add(new AttributeDatum<string>(sectionLocationId, null, CommonTestParameterValues.StringValue, sectionLocation, CommonTestParameterValues.TimeStamp));

            // Act
            var result = NetworkFactory.CreateNetworkFromAttributeDataRecords(attributeData, CommonTestParameterValues.DefaultEquation);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.MaintainableAssets.Count);
        }
    }
}
