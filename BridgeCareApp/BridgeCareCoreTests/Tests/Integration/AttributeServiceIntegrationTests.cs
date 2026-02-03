using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Analysis;
using AssetFox.Core.Data;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.UnitTestsCore.Tests;
using AssetFox.Core.UnitTestsCore.Tests.Attributes;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.TestUtils;
using IamAttribute = AssetFox.Core.Data.Attributes.Attribute;
using AssetFoxCore.Services;
using Xunit;
using AssetFoxCore.Models;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.DataUnitTests.Tests;
using AssetFox.Core.DataUnitTests.TestUtils;

namespace AssetFoxCoreTests.Tests.Integration
{
    public class AttributeServiceIntegrationTests
    {
        private AttributeService CreateAttributeService(UnitOfDataPersistenceWork unitOfWork)
        {
            var cache = new AggregatedSelectValuesResultDtoCache(-1);
            return new AttributeService(unitOfWork, cache);
        }

        [Fact]
        public void GetAttributeSelectValues_NoAggregatedResultsInDatabaseForAttributes_Empty()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var service = CreateAttributeService(TestHelper.UnitOfWork);
            List<string> attributeNames = new() { "NONEXISTANT" };
            var values = service.GetAttributeSelectValues(attributeNames);
            Assert.Empty(values);
        }

        [Fact]
        public void GetAttributeSelectValues_NumericAggregatedResultInDatabase_Warns()
        {
            var dataSource = AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var service = CreateAttributeService(TestHelper.UnitOfWork);
            var attributeNames = new List<string>
            {
                TestAttributeNames.DeckDurationN,
            };
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            AdminSettingsTestSetup.SetupBamsAdminSettingsForTestNetwork(TestHelper.UnitOfWork, false);
            var attribute = AttributeDtos.DeckDurationN;
            var networkId = NetworkTestSetup.NetworkId;
            var assetList = MaintainableAssetLists.SingleInNetwork(networkId, CommonTestParameterValues.DefaultEquation);
            TestHelper.UnitOfWork.MaintainableAssetRepo.CreateMaintainableAssets(assetList, networkId);
            var numericAttribute = AttributeTestSetup.Numeric(attribute.Id, attribute.Name, dataSource.Id);
            var attributeList = new List<IamAttribute> { numericAttribute };
            AggregatedResultTestSetup.SetNumericAggregatedResultsInDb(TestHelper.UnitOfWork, assetList, attributeList);

            var values = service.GetAttributeSelectValues(attributeNames);

            var theValue = values.Single();
            var expectedResultMessage = $"{AttributeService.ValuesForAttribute} {attribute.Name} {AttributeService.IsANumberUseTextInput}";
            var expectedValue = new AttributeSelectValuesResult
            {
                Attribute = attribute.Name,
                ResultMessage = expectedResultMessage,
                ResultType = "success",
                Values = new List<string>(),
            };
            ObjectAssertions.Equivalent(expectedValue, theValue);
        }

        [Fact]
        public void GetAttributeSelectValues_TextAggregatedResultInDatabase_Gets()
        {
            var dataSource = AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var service = CreateAttributeService(TestHelper.UnitOfWork);
            var attributeNames = new List<string>
            {
                TestAttributeNames.Interstate,
            };
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            AdminSettingsTestSetup.SetupBamsAdminSettingsForTestNetwork(TestHelper.UnitOfWork, false);
            var attribute = AttributeDtos.Interstate;
            var networkId = NetworkTestSetup.NetworkId;
            var assetList = MaintainableAssetLists.SingleInNetwork(networkId, CommonTestParameterValues.DefaultEquation);
            TestHelper.UnitOfWork.MaintainableAssetRepo.CreateMaintainableAssets(assetList, networkId);
            var numericAttribute = AttributeTestSetup.Numeric(attribute.Id, attribute.Name, dataSource.Id);
            var attributeList = new List<IamAttribute> { numericAttribute };
            AggregatedResultTestSetup.SetTextAggregatedResultsInDb(TestHelper.UnitOfWork, assetList, attributeList);

            var values = service.GetAttributeSelectValues(attributeNames);

            var theValue = values.Single();
            var expectedResultMessage = $"{AttributeService.ValuesForAttribute} {attribute.Name} {AttributeService.IsANumberUseTextInput}";
            var expectedValue = new AttributeSelectValuesResult
            {
                Attribute = attribute.Name,
                ResultMessage = "Success",
                ResultType = "success",
                Values = new List<string> { "AggregatedResult"},
            };
            ObjectAssertions.Equivalent(expectedValue, theValue);
        }
    }
}
