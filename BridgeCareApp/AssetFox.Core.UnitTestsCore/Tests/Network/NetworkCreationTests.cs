using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.Data.Attributes;
using AppliedResearchAssociates.iAM.Data.ExcelDatabaseStorage.Serializers;
using AppliedResearchAssociates.iAM.Data.Networking;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Migrations;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DataUnitTests;
using AppliedResearchAssociates.iAM.DataUnitTests.Tests;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Attributes;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using Xunit;
using TNetwork = AppliedResearchAssociates.iAM.Data.Networking.Network;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests
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
