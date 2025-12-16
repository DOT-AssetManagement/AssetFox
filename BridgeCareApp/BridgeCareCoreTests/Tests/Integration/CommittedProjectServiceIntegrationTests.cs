using AssetFox.CalculateEvaluate;
using AssetFox.Core.Analysis;
using AssetFox.Core.Data;
using AssetFox.Core.Data.Mappers;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.DataUnitTests;
using AssetFox.Core.DataUnitTests.Tests;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore;
using AssetFox.Core.UnitTestsCore.Tests;
using AssetFox.Core.UnitTestsCore.Tests.Attributes;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.Tests.TreatmentCost;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Services.SummaryReport.CommittedProjects;
using AssetFoxCoreTests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using Xunit;
using IamAttribute = AssetFox.Core.Data.Attributes.Attribute;


namespace AssetFoxCoreTests.Tests.Integration
{
    public class CommittedProjectServiceIntegrationTests
    {
        private CommittedProjectService CreateCommittedProjectService()
        {
            var hubService = HubServiceMocks.Default();
            var service = new CommittedProjectService(TestHelper.UnitOfWork, hubService);
            return service;
        }

        [Fact]
        public void GetTreatmentCost_CriteriaFailToEvaluate_Throws()
        {
            var networkId = Guid.NewGuid();
            var service = CreateCommittedProjectService();
            var assetKeyData = "key";
            var treatmentName = "treatment";
            var keyAttributeId = Guid.NewGuid();
            var maintainableAssets = new List<MaintainableAsset>();
            var assetId = Guid.NewGuid();
            var locationIdentifier = RandomStrings.WithPrefix("Location");
            var location = Locations.Section(locationIdentifier);
            var maintainableAsset = new MaintainableAsset(assetId, networkId, location, "[Deck_Area]");
            var attributeName = RandomStrings.WithPrefix("attribute");
            var attribute = AttributeTestSetup.Text(keyAttributeId, attributeName);
            maintainableAssets.Add(maintainableAsset);
            var network = NetworkTestSetup.ModelForEntityInDbWithNewKeyTextAttribute(
                TestHelper.UnitOfWork, maintainableAssets, networkId, keyAttributeId, attributeName);
            var scenario = SimulationTestSetup.EntityInDb(TestHelper.UnitOfWork, networkId);
            var scenarioId = scenario.Id;
            var attributes = new List<IamAttribute> { attribute };
            AggregatedResultTestSetup.SetTextAggregatedResultsInDb(TestHelper.UnitOfWork,
                maintainableAssets, attributes, assetKeyData);
            var treatmentId = Guid.NewGuid();
            var treatment = TreatmentTestSetup.ModelForSingleTreatmentOfSimulationInDb(
                TestHelper.UnitOfWork, scenarioId, treatmentId, "treatment");
            var treatmentCost = ScenarioTreatmentCostTestSetup.CostForTreatmentInDb(
                TestHelper.UnitOfWork, treatmentId, scenarioId, mergedCriteriaExpression: "ThrowingCriteria");

            var exception = Assert.Throws<CalculateEvaluateCompilationException>(() => service.GetTreatmentCost(
                assetKeyData,
                treatmentId,
                networkId));
            var expectedMessage = @"Unknown reference ""ThrowingCriteria"".";
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void GetTreatmentCost_CriteriaEvaluateToTrue_FindsCost()
        {
            var networkId = Guid.NewGuid();
            var service = CreateCommittedProjectService();
            var assetKeyData = "key";
            var treatmentName = "treatment";
            var keyAttributeId = Guid.NewGuid();
            var maintainableAssets = new List<MaintainableAsset>();
            var assetId = Guid.NewGuid();
            var locationIdentifier = RandomStrings.WithPrefix("Location");
            var location = Locations.Section(locationIdentifier);
            var maintainableAsset = new MaintainableAsset(assetId, networkId, location, "[Deck_Area]");
            var keyAttributeName = RandomStrings.WithPrefix("attribute");
            var keyAttribute = AttributeTestSetup.Text(keyAttributeId, keyAttributeName);
            var resultAttributeName = RandomStrings.WithPrefix("result");
            var resultAttributeId = Guid.NewGuid();
            var resultAttribute = AttributeTestSetup.Text(resultAttributeId, resultAttributeName);
            AttributeTestSetup.CreateSingleTextAttribute(TestHelper.UnitOfWork,
                resultAttributeId, resultAttributeName, ConnectionType.EXCEL, keyAttributeName);
            maintainableAssets.Add(maintainableAsset);
            var network = NetworkTestSetup.ModelForEntityInDbWithNewKeyTextAttribute(
                TestHelper.UnitOfWork, maintainableAssets, networkId, keyAttributeId, keyAttributeName);
            var scenario = SimulationTestSetup.EntityInDb(TestHelper.UnitOfWork, networkId);
            var scenarioId = scenario.Id;
            var attributes = new List<IamAttribute> { keyAttribute, resultAttribute };
            AggregatedResultTestSetup.SetTextAggregatedResultsInDb(TestHelper.UnitOfWork,
                maintainableAssets, attributes, assetKeyData);
            var treatmentId = Guid.NewGuid();
            var treatment = TreatmentTestSetup.ModelForSingleTreatmentOfSimulationInDb(
                TestHelper.UnitOfWork, scenarioId, treatmentId, "treatment");
            var treatmentCost = ScenarioTreatmentCostTestSetup.CostForTreatmentInDb(
                TestHelper.UnitOfWork, treatmentId, scenarioId, mergedCriteriaExpression: $"[{resultAttributeName}]='ok'", equation:"12345");
            var keyAttributes = new List<IamAttribute> { keyAttribute };
            var resultAttributes = new List<IamAttribute> { resultAttribute };
            var resultDictionary = new Dictionary<string, List<IamAttribute>>();
            resultDictionary["ok"] = resultAttributes;
            resultDictionary["key"] = keyAttributes;
            AggregatedResultTestSetup.SetTextAggregatedResultsInDb(TestHelper.UnitOfWork,
                maintainableAssets, resultAttributes, "ok");
            AggregatedResultTestSetup.SetTextAggregatedResultsInDb(TestHelper.UnitOfWork,
                maintainableAssets, resultDictionary);

            var cost = service.GetTreatmentCost(
                assetKeyData,
                treatmentId,
                networkId);

            Assert.Equal(12345, cost);
        }

        [Fact]
        public void DownloadSpreadsheet_ThenReupload_Ok()
        {
            // failing as a part of a test run because MaintainableAssetDataRepository
            // caches KeyProperties.
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var keyAttributeName = TestAttributeNames.BrKey;
            var unusedKeyAttributeName = TestAttributeNames.BmsId;
            var networkId = Guid.NewGuid();
            var treatmentLibraryId = Guid.NewGuid();
            var treatmentLibrary = TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, treatmentLibraryId);
            var assetKeyData = "key";
            var treatmentName = "treatment";
            var keyAttributeId = TestAttributeIds.BrKeyId;
            var keyAttributeDto = AttributeDtos.BrKey;
            var keyAttribute = AttributeDtoDomainMapper.ToDomain(keyAttributeDto, "");
            var maintainableAssets = new List<MaintainableAsset>();
            var assetId = Guid.NewGuid();
            var locationIdentifier = RandomStrings.WithPrefix("Location");
            var location = Locations.Section(locationIdentifier);
            var maintainableAsset = new MaintainableAsset(assetId, networkId, location, "[Deck_Area]");
            var resultAttributeName = RandomStrings.WithPrefix("result");
            var resultAttributeId = Guid.NewGuid();
            var resultAttribute = AttributeTestSetup.Text(resultAttributeId, resultAttributeName, ConnectionType.EXCEL);
            AttributeTestSetup.CreateSingleTextAttribute(TestHelper.UnitOfWork,
                resultAttributeId, resultAttributeName, ConnectionType.EXCEL, keyAttributeName);
            maintainableAssets.Add(maintainableAsset);
            var network = NetworkTestSetup.ModelForEntityInDbWithNewKeyTextAttribute(
                TestHelper.UnitOfWork, maintainableAssets, networkId, keyAttributeId, keyAttributeName);
            TestHelper.UnitOfWork.ClearCachedMaintainableAssetDataRepository();
            var attributeNames = $"{keyAttributeName},{unusedKeyAttributeName}";
            AdminSettingsTestSetup.SetupBamsAdminSettings(TestHelper.UnitOfWork, network.Name, attributeNames, attributeNames);
            var attributes = new List<IamAttribute> { keyAttribute, resultAttribute };
            AggregatedResultTestSetup.SetTextAggregatedResultsInDb(TestHelper.UnitOfWork,
                maintainableAssets, attributes, assetKeyData);

            var budgetLibraryId = Guid.NewGuid();
            var budgetLibraryName = RandomStrings.WithPrefix("BudgetLibrary ");
            var budgetLibrary = BudgetLibraryTestSetup.ModelForEntityInDb(
                TestHelper.UnitOfWork, budgetLibraryName, budgetLibraryId);
            var budgetId = Guid.NewGuid();
            var budget = BudgetTestSetup.AddBudgetToLibrary(TestHelper.UnitOfWork, budgetLibraryId, budgetId);
            var scenarioBudgetId = Guid.NewGuid();
            budget.Id = scenarioBudgetId;

            var treatmentId = Guid.NewGuid();
            var treatment = TreatmentTestSetup.ModelForSingleTreatmentOfLibraryInDb(
                TestHelper.UnitOfWork, treatmentLibraryId, treatmentId, treatmentName);
            var treatmentCost = LibraryTreatmentCostTestSetup.ModelForEntityInDb(
                TestHelper.UnitOfWork, treatmentId, treatmentLibraryId, mergedCriteriaExpression: $"[{resultAttributeName}]='ok'");

            var keyAttributes = new List<IamAttribute> { keyAttribute };
            var simulationEntity = SimulationTestSetup.EntityInDb(TestHelper.UnitOfWork, networkId);
            var simulationId = simulationEntity.Id;
            InvestmentPlanTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId, null, 2023);
            ScenarioBudgetTestSetup.UpsertOrDeleteScenarioBudgets(
               TestHelper.UnitOfWork, new List<BudgetDTO> { budget }, simulationId);
            var treatments = new List<TreatmentDTO> { treatment };
            TestHelper.UnitOfWork.SelectableTreatmentRepo.UpsertOrDeleteScenarioSelectableTreatment(treatments, simulationId);

            var committedProjectId = Guid.NewGuid();
            var committedProject = SectionCommittedProjectDtos.Dto(
                committedProjectId,
                scenarioBudgetId,
                simulationId,
                ProjectSourceDTO.None,
                treatmentName,
                keyAttributeName,
                location.LocationIdentifier,
                2023);
            committedProject.Cost = 31415926;
            List<SectionCommittedProjectDTO> sectionCommittedProjects = new List<SectionCommittedProjectDTO> { committedProject };
            TestHelper.UnitOfWork.CommittedProjectRepo.UpsertCommittedProjects(sectionCommittedProjects);

            var committedProjects1 = TestHelper.UnitOfWork.CommittedProjectRepo.GetSectionCommittedProjectDTOs(simulationId);
            var committedProjectIds = new List<Guid> { committedProject.Id };
            var service = CreateCommittedProjectService();

            // first act
            var fileInfo = service.ExportCommittedProjectsFile(simulationId);
            var dataAsString = fileInfo.FileData;
            var bytes = Convert.FromBase64String(dataAsString);
            var stream = new MemoryStream(bytes);
            //File.WriteAllBytes("zzzzz.xlsx", bytes);
            var excelPackage = new ExcelPackage(stream);
            TestHelper.UnitOfWork.CommittedProjectRepo.DeleteSpecificCommittedProjects(committedProjectIds);
            var committedProjects2 = TestHelper.UnitOfWork.CommittedProjectRepo.GetSectionCommittedProjectDTOs(simulationId);
            Assert.Empty(committedProjects2);
            var formFile = FormFiles.FromFileInfo(fileInfo);

            //second act
            var serviceProvider = ServiceProviders.AdminControllersWithSimulationIdAndFiles(simulationId, formFile);
            var service2 = serviceProvider.GetService<ICommittedProjectService>();
            service2.ImportCommittedProjectFiles(simulationId, excelPackage, fileInfo.FileName, "Ignored user id");
            var committedProjects3 = TestHelper.UnitOfWork.CommittedProjectRepo.GetSectionCommittedProjectDTOs(simulationId);
            var id1 = committedProjects1[0].LocationKeys["ID"];
            var id3 = committedProjects3[0].LocationKeys["ID"];
            ObjectAssertions.EquivalentExcluding(committedProjects1, committedProjects3, x => x[0].LocationKeys, x => x[0].Id);
            Assert.NotEqual(id1, id3);
        }

        [Fact]
        public void DownloadTemplate_IsValidExcelPackage()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var networkId = Guid.NewGuid();
            var keyAttributeId = TestAttributeIds.BrKeyId;
            var maintainableAssets = new List<MaintainableAsset>();
            var assetId = Guid.NewGuid();
            var locationIdentifier = RandomStrings.WithPrefix("Location");
            var location = Locations.Section(locationIdentifier);
            var maintainableAsset = new MaintainableAsset(assetId, networkId, location, "[Deck_Area]");
            var keyAttributeName = TestAttributeNames.BrKey;

            maintainableAssets.Add(maintainableAsset);
            var network = NetworkTestSetup.ModelForEntityInDbWithNewKeyTextAttribute(
                TestHelper.UnitOfWork, maintainableAssets, networkId, keyAttributeId, keyAttributeName);
            AdminSettingsTestSetup.SetupBamsAdminSettings(TestHelper.UnitOfWork, network.Name, keyAttributeName, keyAttributeName);
            var service = CreateCommittedProjectService();

            var fileInfo = service.CreateCommittedProjectTemplate(network.Id);

            ExcelPackageAsserts.ValidExcelPackageData(fileInfo);
        }
    }
}
