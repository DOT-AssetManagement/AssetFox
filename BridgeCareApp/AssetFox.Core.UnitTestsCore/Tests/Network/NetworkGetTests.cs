using System;
using TNetwork = AssetFox.Core.Data.Networking.Network;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.TestHelpers;
using Xunit;
using AssetFox.Core.UnitTestsCore.Tests.Attributes;
using AssetFox.Core.Data.Attributes;
using AssetFox.Core.DataUnitTests.Tests;
using AssetFox.Core.DataUnitTests;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers;
using IamAttribute = AssetFox.Core.Data.Attributes.Attribute;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.Data.Aggregation;
using AssetFox.Core.Data.Mappers;
using AssetFox.Core.DataUnitTests.TestUtils;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public class NetworkGetTests
    {

        private void RunGetSimulationAnalysisNetwork_NetworkInDb_Does(int assetCount, int aggregatedResultPerAssetCount)
        {
            var networkId = Guid.NewGuid();
            var maintainableAssets = new List<MaintainableAsset>();
            for (int i = 0; i < assetCount; i++)
            {
                var maintainableAsset = MaintainableAssets.InNetwork(networkId, CommonTestParameterValues.DefaultEquation);
                maintainableAssets.Add(maintainableAsset);
            }
            var resultAttributes = new List<IamAttribute>();
            for (int i = 0; i < aggregatedResultPerAssetCount; i++)
            {
                var resultAttribute = AttributeTestSetup.Numeric();
                resultAttributes.Add(resultAttribute);
            }
            TestHelper.UnitOfWork.AttributeRepo.UpsertAttributesNonAtomic(resultAttributes);
            var network = NetworkTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, maintainableAssets, networkId);
            AggregatedResultTestSetup.SetNumericAggregatedResultsInDb(TestHelper.UnitOfWork, maintainableAssets, resultAttributes);

            var config = TestConfiguration.Get();
            var connectionString = TestConnectionStrings.BridgeCare(config);
            var dataSourceDto = DataSourceTestSetup.DtoForSqlDataSourceInDb(TestHelper.UnitOfWork, connectionString);
            var districtAttributeDomain = AttributeConnectionAttributes.String(connectionString, dataSourceDto.Id);
            var districtAttribute = AttributeDtoDomainMapper.ToDto(districtAttributeDomain, dataSourceDto);
            UnitTestsCoreAttributeTestSetup.EnsureAttributeExists(districtAttribute);
            var explorer = TestHelper.UnitOfWork.AttributeRepo.GetExplorer();

            var simulationAnalysisNetwork = TestHelper.UnitOfWork.NetworkRepo.GetSimulationAnalysisNetwork(network.Id, explorer);

            Assert.Equal(network.Id, simulationAnalysisNetwork.Id);
            var assets = simulationAnalysisNetwork.Assets;
            Assert.Equal(assetCount, assets.Count);
            foreach (var asset in assets)
            {
                var historicalAttributeList = asset.HistoricalAttributes.ToList();
                Assert.Equal(aggregatedResultPerAssetCount, historicalAttributeList.Count);
            }
        }


        [Fact]
        public void GetSimulationAnalysisNetwork_NetworkInDb300Assets_Does()
        {
            RunGetSimulationAnalysisNetwork_NetworkInDb_Does(300, 10);
        }
    }
}
