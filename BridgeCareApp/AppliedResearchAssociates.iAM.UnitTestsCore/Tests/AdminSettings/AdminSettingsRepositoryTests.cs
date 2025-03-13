using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AppliedResearchAssociates.iAM.Data;
using AppliedResearchAssociates.iAM.Data.Networking;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL;
using AppliedResearchAssociates.iAM.DataUnitTests.Tests;
using AppliedResearchAssociates.iAM.DataUnitTests.TestUtils;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Repositories;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using Xunit;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests.AdminSettings
{
    public class AdminSettingsRepositoryTests
    {
        [Fact]
        public void SetConstraintType_ThenGet_Same()
        {
            var constraintType = RandomStrings.WithPrefix("constraintType");
            TestHelper.UnitOfWork.AdminSettingsRepo.SetConstraintType(constraintType);
            var constraintTypeAfter = TestHelper.UnitOfWork.AdminSettingsRepo.GetConstraintType();
            Assert.Equal(constraintTypeAfter, constraintType);
        }

        [Fact]
        public void SetAssetType_ThenGet_Same()
        {
            var assetType = RandomStrings.WithPrefixAnd2CharSuffix("AssetType");
            TestHelper.UnitOfWork.AdminSettingsRepo.SetAssetType(assetType);
            var assetTypeAfter = TestHelper.UnitOfWork.AdminSettingsRepo.GetAssetType();
            var firstAssetTypeAfter = assetTypeAfter.Single();
            Assert.Equal(assetType, firstAssetTypeAfter);
        }

        [Fact]
        public void SetTwoAssetTypes_ThenGet_Same()
        {
            var assetType1 = RandomStrings.WithPrefixAnd2CharSuffix("AssetType1");
            var assetType2 = RandomStrings.WithPrefixAnd2CharSuffix("AssetType2");
            var assetTypes = new List<string> { assetType1, assetType2 };
            var assetTypesString = $"{assetType1},{assetType2}";

            TestHelper.UnitOfWork.AdminSettingsRepo.SetAssetType(assetTypesString);
            var assetTypesAfter = TestHelper.UnitOfWork.AdminSettingsRepo.GetAssetType();

            Assert.Equivalent(assetTypes, assetTypesAfter);
        }

        [Fact]
        public void SetSimulationReports_ThenGet_Same()
        {
            var reportName1 = RandomStrings.WithPrefixAnd2CharSuffix("Report1");
            var reportName2 = RandomStrings.WithPrefixAnd2CharSuffix("Report2");
            var reportNamesString = $"{reportName1},{reportName2}";

            TestHelper.UnitOfWork.AdminSettingsRepo.SetSimulationReports(reportNamesString);
            var reportNamesAfter = TestHelper.UnitOfWork.AdminSettingsRepo.GetSimulationReportNames();

            var expected = new List<string>{reportName1, reportName2};
            ObjectAssertions.Equivalent(expected, reportNamesAfter);
        }

        [Fact]
        public void SetImplementationName_ThenGet_Same()
        {
            var implementationName = RandomStrings.WithPrefixAnd2CharSuffix("ImplementationName");

            TestHelper.UnitOfWork.AdminSettingsRepo.SetImplementationName(implementationName);
            var implementationNameAfter = TestHelper.UnitOfWork.AdminSettingsRepo.GetImplementationName();

            Assert.Equal(implementationName, implementationNameAfter);
        }

        [Fact]
        public void SetPrimaryNetwork_ThenGetPrimaryNetworkId_Expected()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);

            TestHelper.UnitOfWork.AdminSettingsRepo.SetPrimaryNetwork(NetworkTestSetup.TestNetworkName);

            var primaryNetworkId = TestHelper.UnitOfWork.AdminSettingsRepo.GetPrimaryNetworkId();
            Assert.Equal(NetworkTestSetup.NetworkId, primaryNetworkId);
        }

        [Fact]
        public void SetRawDataNetwork_ThenGetPrimaryNetworkId_Expected()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);

            TestHelper.UnitOfWork.AdminSettingsRepo.SetRawDataNetwork(NetworkTestSetup.TestNetworkName);

            var primaryNetworkId = TestHelper.UnitOfWork.AdminSettingsRepo.GetRawDataNetworkId();
            Assert.Equal(NetworkTestSetup.NetworkId, primaryNetworkId);
        }

        [Fact]
        public void CreateAgencyLogo_Does()
        {
            var logoString = "agenlogo"; // length has to be a multiple of 4
            var logoBytes = Convert.FromBase64String(logoString);

            TestHelper.UnitOfWork.AdminSettingsRepo.SetAgencyLogo(logoBytes);

            var fetchedLogo = TestHelper.UnitOfWork.AdminSettingsRepo.GetAgencyLogo();
            Assert.EndsWith(logoString, fetchedLogo);
        }

        [Fact]
        public void ChangeAgencyLogo_Does()
        {
            var logoString1 = "agenlog1"; // length has to be a multiple of 4
            var logoString2 = "agenlog2"; // length has to be a multiple of 4
            var logoBytes1 = Convert.FromBase64String(logoString1);
            var logoBytes2 = Convert.FromBase64String(logoString2);

            TestHelper.UnitOfWork.AdminSettingsRepo.SetAgencyLogo(logoBytes1);
            TestHelper.UnitOfWork.AdminSettingsRepo.SetAgencyLogo(logoBytes2);

            var fetchedLogo = TestHelper.UnitOfWork.AdminSettingsRepo.GetAgencyLogo();
            Assert.EndsWith(logoString2, fetchedLogo);
        }

        [Fact]
        public void CreateImplementationLogo_Does()
        {
            var logoString = "implementatilogo";
            var logoBytes = Convert.FromBase64String(logoString);

            TestHelper.UnitOfWork.AdminSettingsRepo.SetImplementationLogo(logoBytes);

            var fetchedLogo = TestHelper.UnitOfWork.AdminSettingsRepo.GetImplementationLogo ();
            Assert.EndsWith(logoString, fetchedLogo);
        }

        [Fact]
        public void ChangeImplementationLogo_Does()
        {
            var logoString1 = "implementatilog1"; // length has to be a multiple of 4
            var logoString2 = "implementatilog2"; // length has to be a multiple of 4
            var logoBytes1 = Convert.FromBase64String(logoString1);
            var logoBytes2 = Convert.FromBase64String(logoString2);

            TestHelper.UnitOfWork.AdminSettingsRepo.SetImplementationLogo(logoBytes1);
            TestHelper.UnitOfWork.AdminSettingsRepo.SetImplementationLogo(logoBytes2);

            var fetchedLogo = TestHelper.UnitOfWork.AdminSettingsRepo.GetImplementationLogo();
            Assert.EndsWith(logoString2, fetchedLogo);
        }

        [Fact]
        public void SetPrimaryNetwork_Does()
        {
            var networkEntity = NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);

            TestHelper.UnitOfWork.AdminSettingsRepo.SetPrimaryNetwork(networkEntity.Name);

            var primaryNetwork = TestHelper.UnitOfWork.AdminSettingsRepo.GetPrimaryNetwork();
            Assert.Equal(networkEntity.Name, primaryNetwork);
        }

        [Fact]
        public void ChangeMainNetwork_Does()
        {
            var networkEntity1 = NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var keyAttributeName = RandomStrings.WithPrefix("keyAttribute");
            var keyAttributeId = Guid.NewGuid();
            var networkId = Guid.NewGuid();
            var maintainableAssets = MaintainableAssetLists.SingleInNetwork(networkId, CommonTestParameterValues.DefaultEquation);
            var resultAttributeName = RandomStrings.WithPrefix("result");
            var resultAttributeId = Guid.NewGuid();
            AttributeTestSetup.CreateSingleTextAttribute(TestHelper.UnitOfWork,
                resultAttributeId, resultAttributeName, ConnectionType.EXCEL, keyAttributeName);
            var networkEntity2 = NetworkTestSetup.ModelForEntityInDbWithNewKeyTextAttribute(
                TestHelper.UnitOfWork, maintainableAssets, networkId, keyAttributeId, keyAttributeName);

            TestHelper.UnitOfWork.AdminSettingsRepo.SetPrimaryNetwork(networkEntity1.Name);
            var primaryNetworkBefore = TestHelper.UnitOfWork.NetworkRepo.GetMainNetwork();
            Assert.Equal(networkEntity1.Name, primaryNetworkBefore.Name);

            TestHelper.UnitOfWork.AdminSettingsRepo.SetPrimaryNetwork(networkEntity2.Name);
            var primaryNetworkAfter = TestHelper.UnitOfWork.NetworkRepo.GetMainNetwork();
            Assert.Equal(networkEntity2.Name, primaryNetworkAfter.Name);

            TestHelper.UnitOfWork.AdminSettingsRepo.SetRawDataNetwork(networkEntity1.Name);
            var rawDataNetworkBefore = TestHelper.UnitOfWork.NetworkRepo.GetRawNetwork();
            Assert.Equal(networkEntity1.Name, rawDataNetworkBefore.Name);

            TestHelper.UnitOfWork.AdminSettingsRepo.SetRawDataNetwork(networkEntity2.Name);
            var rawDataNetworkAfter = TestHelper.UnitOfWork.NetworkRepo.GetRawNetwork();
            Assert.Equal(networkEntity2.Name, rawDataNetworkAfter.Name);
        }

        [Fact]
        public void SetRawDataNetwork_Does()
        {
            var networkEntity = NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);

            TestHelper.UnitOfWork.AdminSettingsRepo.SetRawDataNetwork(networkEntity.Name);

            var rawDataNetwork = TestHelper.UnitOfWork.AdminSettingsRepo.GetRawDataNetwork();
            Assert.Equal(networkEntity.Name, rawDataNetwork);
        }

        [Fact]
        public void DeleteAdminSetting_Does()
        {
            var networkEntity = NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            TestHelper.UnitOfWork.AdminSettingsRepo.SetPrimaryNetwork(networkEntity.Name);
            var primaryNetwork = TestHelper.UnitOfWork.AdminSettingsRepo.GetPrimaryNetwork();
            Assert.Equal(networkEntity.Name, primaryNetwork);

            TestHelper.UnitOfWork.AdminSettingsRepo.DeleteAdminSetting(AdminSettingsRepository.primaryNetworkKey);
            var primaryNetworkAfter = TestHelper.UnitOfWork.AdminSettingsRepo.GetPrimaryNetwork();
            Assert.Null(primaryNetworkAfter);
        }

        [Fact]
        public void SetOneKeyField_ThenGet_Same()
        {
            var attributeName = RandomStrings.WithPrefix("attribute");
            AttributeTestSetup.CreateSingleTextAttribute(TestHelper.UnitOfWork, name: attributeName);
            var keyFields = attributeName;
            TestHelper.UnitOfWork.AdminSettingsRepo.SetKeyFields(keyFields);

            var keyFieldsAfter = TestHelper.UnitOfWork.AdminSettingsRepo.GetKeyFields();
            var keyFieldAfter = keyFieldsAfter.Single();

            Assert.Equal(keyFields, keyFieldAfter);
        }

        [Fact]
        public void SetTwoKeyFields_ThenGet_Same()
        {
            var attributeName1 = RandomStrings.WithPrefix("attribute");
            var attributeName2 = RandomStrings.WithPrefix("attribute");
            AttributeTestSetup.CreateSingleTextAttribute(TestHelper.UnitOfWork, name: attributeName1);
            AttributeTestSetup.CreateSingleTextAttribute(TestHelper.UnitOfWork, name: attributeName2);
            var keyFields = $"{attributeName1},{attributeName2}";
            TestHelper.UnitOfWork.AdminSettingsRepo.SetKeyFields(keyFields);

            var keyFieldsAfter = TestHelper.UnitOfWork.AdminSettingsRepo.GetKeyFields();
            var expectedKeyFieldsAfter = new List<string> { attributeName1, attributeName2 };
            Assert.Equal(expectedKeyFieldsAfter, keyFieldsAfter);
        }

        [Fact]
        public void ChangeKeyFields_Does()
        {
            var attributeName1 = RandomStrings.WithPrefix("attribute");
            var attributeName2 = RandomStrings.WithPrefix("attribute");
            AttributeTestSetup.CreateSingleTextAttribute(TestHelper.UnitOfWork, name: attributeName1);
            AttributeTestSetup.CreateSingleTextAttribute(TestHelper.UnitOfWork, name: attributeName2);
            var keyFields1 = $"{attributeName1}";
            var keyFields2 = $"{attributeName2}";
            TestHelper.UnitOfWork.AdminSettingsRepo.SetKeyFields(keyFields1);

            TestHelper.UnitOfWork.AdminSettingsRepo.SetKeyFields(keyFields2);

            var keyFieldsAfter = TestHelper.UnitOfWork.AdminSettingsRepo.GetKeyFields();
            var expectedKeyFieldsAfter = new List<string> { attributeName2 };
            Assert.Equal(expectedKeyFieldsAfter, keyFieldsAfter);
        }

        [Fact]
        public void SetOneRawKeyField_ThenGet_Same()
        {
            var attributeName = RandomStrings.WithPrefix("attribute");
            AttributeTestSetup.CreateSingleTextAttribute(TestHelper.UnitOfWork, name: attributeName);
            var keyFields = attributeName;
            TestHelper.UnitOfWork.AdminSettingsRepo.SetRawDataKeyFields(keyFields);

            var keyFieldsAfter = TestHelper.UnitOfWork.AdminSettingsRepo.GetRawKeyFields();
            var keyFieldAfter = keyFieldsAfter.Single();

            Assert.Equal(keyFields, keyFieldAfter);
        }

        [Fact]
        public void SetTwoRawKeyFields_ThenGet_Same()
        {
            var attributeName1 = RandomStrings.WithPrefix("attribute");
            var attributeName2 = RandomStrings.WithPrefix("attribute");
            AttributeTestSetup.CreateSingleTextAttribute(TestHelper.UnitOfWork, name: attributeName1);
            AttributeTestSetup.CreateSingleTextAttribute(TestHelper.UnitOfWork, name: attributeName2);
            var keyFields = $"{attributeName1},{attributeName2}";
            TestHelper.UnitOfWork.AdminSettingsRepo.SetRawDataKeyFields(keyFields);

            var keyFieldsAfter = TestHelper.UnitOfWork.AdminSettingsRepo.GetRawKeyFields();
            var keyFieldsAfter2 = TestHelper.UnitOfWork.AdminSettingsRepo.GetRawDataKeyFields();
            var expectedKeyFieldsAfter = new List<string> { attributeName1, attributeName2 };
            Assert.Equal(expectedKeyFieldsAfter,keyFieldsAfter);
        }

        [Fact]
        public void SetInventoryReports_ThenGet_Same()
        {
            var inventoryReport = RandomStrings.WithPrefixAnd2CharSuffix("InventoryReports");

            TestHelper.UnitOfWork.AdminSettingsRepo.SetInventoryReports(inventoryReport);
            var inventoryReportsAfter = TestHelper.UnitOfWork.AdminSettingsRepo.GetInventoryReports();

            var singleInventoryReportsAfter = inventoryReportsAfter.Single();
            Assert.Equal(inventoryReport, singleInventoryReportsAfter);
        }

        [Fact]
        public void SetImplementationLogoImage_ThenGet_ExpectedInitialSubstring()
        {
            using var image = Images.Image(50, 1, Color.AliceBlue);
            TestHelper.UnitOfWork.AdminSettingsRepo.SetImplementationLogo(image, "image/png");

            var imageAfter = TestHelper.UnitOfWork.AdminSettingsRepo.GetImplementationLogo();
            Assert.StartsWith("data:image/png;base64", imageAfter);
        }

        [Fact]
        public void ChangeRawKeyFields_Does()
        {
            var attributeName1 = RandomStrings.WithPrefix("attribute");
            var attributeName2 = RandomStrings.WithPrefix("attribute");
            AttributeTestSetup.CreateSingleTextAttribute(TestHelper.UnitOfWork, name: attributeName1);
            AttributeTestSetup.CreateSingleTextAttribute(TestHelper.UnitOfWork, name: attributeName2);
            var keyFields1 = $"{attributeName1}";
            var keyFields2 = $"{attributeName2}";
            TestHelper.UnitOfWork.AdminSettingsRepo.SetRawDataKeyFields(keyFields1);

            TestHelper.UnitOfWork.AdminSettingsRepo.SetRawDataKeyFields(keyFields2);

            var keyFieldsAfter = TestHelper.UnitOfWork.AdminSettingsRepo.GetRawKeyFields();
            var expectedKeyFieldsAfter = new List<string> { attributeName2 };
            Assert.Equal(expectedKeyFieldsAfter, keyFieldsAfter);
        }

        [Fact]
        // WJPRQ -- There are two code paths in the method under test here.
        // This brings up the question of multiple tests on the method.
        // This applies to many methods in AdminSettingsRepository.
        public void SetAdminContactEmail_ThenGet_Same()
        {
            var email = RandomStrings.WithPrefixAnd2CharSuffix("email");

            TestHelper.UnitOfWork.AdminSettingsRepo.SetAdminContactEmail(email);
            var emailAfter = TestHelper.UnitOfWork.AdminSettingsRepo.GetAdminContactEmail();

            Assert.Equal(email, emailAfter);
        }
    }
}
