using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetFox.Core.Data.Attributes;
using AssetFox.Core.Data.ExcelDatabaseStorage.Serializers;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.DataPersistenceCore.Migrations;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DataUnitTests;
using AssetFox.Core.DataUnitTests.Tests;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.Tests.Attributes;
using AssetFox.Core.UnitTestsCore.TestUtils;
using Xunit;
using TNetwork = AssetFox.Core.Data.Networking.Network;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public class NetworkCreationTests
    {
        [Fact]
        public async Task CreateNetworkViaFactoryAndRepository_Does()
        {
            var networkName = RandomStrings.WithPrefix("Network");
            var config = TestConfiguration.Get();
            var allNetworksBefore = await TestHelper.UnitOfWork.NetworkRepo.Networks();
            var dataSourceDto = DataSourceTestSetup.DtoForExcelDataSourceInDb(TestHelper.UnitOfWork);
            var attribute = UnitTestsCoreAttributeTestSetup.ExcelAttributeForEntityInDb(dataSourceDto);
            var importedSpreadsheet = ExcelRawDataSetup.RawData(dataSourceDto.Id);
            var deserializationResult = ExcelRawDataSpreadsheetSerializer.Deserialize(importedSpreadsheet.SerializedWorksheetContent);
            var rawDataSpreadsheet = deserializationResult.Worksheet;
            var defaultEquation = "[Deck_Area]";
            var attribute2 = AttributeConnectionAttributes.ForExcelTestData(dataSourceDto.Id);
            var attributeConnection = new ExcelAttributeConnection(attribute2, dataSourceDto, rawDataSpreadsheet);
            // var attributeConnection = AttributeConnectionBuilder.Build(textAttribute, dataSourceDto, TestHelper.UnitOfWork);
            var data = attributeConnection.GetData<string>();
            var network = NetworkFactory.CreateNetworkFromAttributeDataRecords(
                  data, defaultEquation);
            network.Name = networkName;
            var networkId = network.Id;

            // insert network domain data into the data source
            TestHelper.UnitOfWork.NetworkRepo.CreateNetwork(network);

            var networkIds = new List<Guid> { networkId };
            var allNetworks = await TestHelper.UnitOfWork.NetworkRepo.Networks();
            var actual = allNetworks.Single(n => n.Id == networkId);
            Assert.Equal(allNetworksBefore.Count + 1, allNetworks.Count);
            Assert.Equal(networkName, actual.Name);
        }
    }
}
