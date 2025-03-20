using System.Threading.Channels;
using AppliedResearchAssociates.iAM.Data;
using AppliedResearchAssociates.iAM.Data.Mappers;
using AppliedResearchAssociates.iAM.Data.Networking;
using AppliedResearchAssociates.iAM.DataUnitTests;
using AppliedResearchAssociates.iAM.DataUnitTests.Tests;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Attributes;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using BridgeCareCore.Models;
using BridgeCareCore.Services.Aggregation;
using Xunit;

namespace BridgeCareCoreTests.Tests.Integration
{
    public class SqlAggregationTests
    {
        [Fact]
        public async Task Aggregate_SqlDataSourceInDb_AttributesInDb_Aggregates()
        {
            // We may at some point drop sql aggregation entirely.
            // Until then, keep this around.
            var config = TestConfiguration.Get();
            var connectionString = TestConnectionStrings.BridgeCare(config);
            var dataSourceDto = DataSourceTestSetup.DtoForSqlDataSourceInDb(TestHelper.UnitOfWork, connectionString);
            var districtAttributeDomain = AttributeConnectionAttributes.String(connectionString, dataSourceDto.Id);
            var districtAttribute = AttributeDtoDomainMapper.ToDto(districtAttributeDomain, dataSourceDto);
            UnitTestsCoreAttributeTestSetup.EnsureAttributeExists(districtAttribute);

            var networkName = RandomStrings.WithPrefix("Network");
            var allDataSourceDto = AllDataSourceDtoFakeFrontEndFactory.ToAll(dataSourceDto);

            var networkDefinitionAttribute = AllAttributeDtos.BrKey(allDataSourceDto);
            var parameters = new NetworkCreationParameters
            {
                DefaultEquation = "[Deck_Area]",
                NetworkDefinitionAttribute = networkDefinitionAttribute
            };
            var network = NetworkIntegrationTestSetup.ModelForEntityInDbViaFactory(
                TestHelper.UnitOfWork, districtAttributeDomain, parameters, networkName);

            var networkId = network.Id;
            var assetName = "AssetName";
            var location = new SectionLocation(Guid.NewGuid(), assetName);
            var maintainableAssetId = Guid.NewGuid();
            var spatialWeightingValue = "[Deck_Area]";
            var newAsset = new MaintainableAsset(maintainableAssetId, networkId, location, spatialWeightingValue);
            var assetList = new List<MaintainableAsset> { newAsset };
            TestHelper.UnitOfWork.MaintainableAssetRepo.CreateMaintainableAssets(assetList, networkId);
            var aggregationService = new AggregationService(TestHelper.UnitOfWork);
            var channel = Channel.CreateUnbounded<AggregationStatusMemo>();
            var aggregationState = new AggregationState();
            var attributes = new List<AttributeDTO> { districtAttribute };

            var aggregationResult = await aggregationService.AggregateNetworkData(channel.Writer, networkId, aggregationState, attributes);

            Assert.True(aggregationResult);
            var attributeNames = new List<string> { districtAttribute.Name };
            var aggregatedValues = TestHelper.UnitOfWork.AggregatedResultRepo.GetAggregatedResultsForAttributeNames(networkId, attributeNames);
            var aggregatedValue = aggregatedValues.Single();
            var textValue = aggregatedValue.TextValue;
            var textValues = new List<string> { textValue };
            var attributeWithMatchingName = TestHelper.UnitOfWork.AttributeRepo.GetAttributesWithNames(textValues);
            Assert.Single(attributeWithMatchingName); // This passes because of the attribute's command. If the command changes, this will need to change too.
        }
    }
}
