using AssetFox.Core.Data;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.DataUnitTests.Tests;
using AssetFox.Core.DataUnitTests.TestUtils;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore;
using AssetFox.Core.UnitTestsCore.Tests;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.Tests.TreatmentCost;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Controllers;
using AssetFoxCore.Models;
using AssetFoxCore.Services;
using AssetFoxCore.Services.SummaryReport.CommittedProjects;
using AssetFoxCoreTests.Helpers;
using AssetFoxCoreTests.Tests.General_Work_Queue;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Xunit;
using IamAttribute = AssetFox.Core.Data.Attributes.Attribute;

namespace AssetFoxCoreTests.Tests.Integration
{
    public class CommittedProjectControllerIntegrationTests
    {

        [Fact]
        public async Task ExportCommittedProjects_ThenImport_Expected()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            AdminSettingsTestSetup.SetupBamsAdminSettingsForTestNetwork(TestHelper.UnitOfWork, true);
            var simulationId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
            InvestmentPlanTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId, null, 2025);
            var treatmentId = Guid.NewGuid();
            var treatmentName = RandomStrings.WithPrefix("treatment");
            var treatment = TreatmentTestSetup.ModelForSingleTreatmentOfSimulationInDb(TestHelper.UnitOfWork, simulationId, treatmentId, treatmentName);
            var scenarioBudgetId = Guid.NewGuid();
            var budget = BudgetDtos.New(scenarioBudgetId);
            var budgets = new List<BudgetDTO> { budget };
            ScenarioBudgetTestSetup.UpsertOrDeleteScenarioBudgets(TestHelper.UnitOfWork, budgets, simulationId);
            var controller = CreateController();
            var locationKey = TestAttributeNames.BrKey;
            var locationInteger = RandomIntegers.PositiveNotRepeated();
            var locationValue = locationInteger.ToString();
            var assetId = Guid.NewGuid();
            var sectionLocation = Locations.Section(locationValue);
            var maintainableAsset = MaintainableAssets.InNetwork(NetworkTestSetup.NetworkId, TestAttributeNames.BrKey, assetId, sectionLocation);
            var maintainableAssets = new List<MaintainableAsset> { maintainableAsset };
            TestHelper.UnitOfWork.MaintainableAssetRepo.CreateMaintainableAssets(maintainableAssets, NetworkTestSetup.NetworkId);
            var committedProject = CommittedProjectTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, scenarioBudgetId, simulationId, locationKey, locationValue, treatmentName, 2025);
            var committedProjectsBefore = TestHelper.UnitOfWork.CommittedProjectRepo.GetSectionCommittedProjectDTOs(simulationId);
            Assert.NotEmpty(committedProjectsBefore);

            // act 1
            var exportResult = await controller.ExportCommittedProjects(simulationId);
            TestHelper.UnitOfWork.CommittedProjectRepo.DeleteSimulationCommittedProjects(simulationId);
            var fileInfo = ActionResultAssertions.OkObject<FileInfoDTO>(exportResult);
            var formFile = FormFiles.FromFileInfo(fileInfo);
            var serviceProvider = ServiceProviders.AdminControllersWithSimulationIdAndFiles(simulationId, formFile);
            var controller2 = CreateController(serviceProvider);

            // act 2
            var importResult = await controller2.ImportCommittedProjects();
            ActionResultAssertions.Ok(importResult);

            // act 3
            var workStarter = await serviceProvider.DequeueAndCompleteHiddenUploadQueueTask();
            var committedProjectsAfter = TestHelper.UnitOfWork.CommittedProjectRepo.GetSectionCommittedProjectDTOs(simulationId);
            Assert.NotEmpty(committedProjectsAfter);
        }

        private CommittedProjectController CreateController()
        {
            var hubService = HubServiceMocks.Default();
            var service = new CommittedProjectService(TestHelper.UnitOfWork, hubService);
            var pagingService = new CommittedProjectPagingService(TestHelper.UnitOfWork);
            var security = EsecSecurityMocks.Admin;
            var contextAccessor = HttpContextAccessorMocks.Default();
            var claimHelper = ClaimHelperMocks.New();
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            return new CommittedProjectController(
                service,
                pagingService,
                security,
                TestHelper.UnitOfWork,
                hubService,
                contextAccessor,
                claimHelper.Object,
                generalWorkQueue.Object
                );
        }

        private CommittedProjectController CreateController(
            IServiceProvider serviceProvider
        )
        {
            var controller = serviceProvider.GetControllerWithUnifiedHttpContext<CommittedProjectController>();
            return controller;
        }

        [Fact]
        public async Task FillTreatmentValues_Does()
        {
            var networkId = Guid.NewGuid();
            var assetKeyData = "key";
            var treatmentName = "treatment";
            var keyAttributeId = Guid.NewGuid();
            var maintainableAssets = MaintainableAssetLists.SingleInNetwork(networkId, CommonTestParameterValues.DefaultEquation);
            var keyAttributeName = RandomStrings.WithPrefix("attribute");
            var keyAttribute = AttributeTestSetup.Text(keyAttributeId, keyAttributeName);
            var resultAttributeName = RandomStrings.WithPrefix("result");
            var resultAttributeId = Guid.NewGuid();
            var resultAttribute = AttributeTestSetup.Text(resultAttributeId, resultAttributeName);
            AttributeTestSetup.CreateSingleTextAttribute(TestHelper.UnitOfWork,
                resultAttributeId, resultAttributeName, ConnectionType.EXCEL, keyAttributeName);
            var network = NetworkTestSetup.ModelForEntityInDbWithNewKeyTextAttribute(
                TestHelper.UnitOfWork, maintainableAssets, networkId, keyAttributeId, keyAttributeName);
            var scenario = SimulationTestSetup.EntityInDb(TestHelper.UnitOfWork, networkId);
            var scenarioId = scenario.Id;
            var attributes = new List<IamAttribute> { keyAttribute, resultAttribute };
            AggregatedResultTestSetup.SetTextAggregatedResultsInDb(TestHelper.UnitOfWork,
                maintainableAssets, attributes, assetKeyData);
            var treatmentId = Guid.NewGuid();
            var treatment = TreatmentTestSetup.ModelForSingleTreatmentOfSimulationInDb(TestHelper.UnitOfWork, scenarioId, treatmentId, name: treatmentName);
            var treatmentCost = ScenarioTreatmentCostTestSetup.CostForTreatmentInDb(TestHelper.UnitOfWork, treatmentId, scenarioId,
                mergedCriteriaExpression: $"[{resultAttributeName}]='ok'", equation: "12345");
            var keyAttributes = new List<IamAttribute> { keyAttribute };
            var resultAttributes = new List<IamAttribute> { resultAttribute };
            var resultDictionary = new Dictionary<string, List<IamAttribute>>();
            resultDictionary["ok"] = resultAttributes;
            resultDictionary["key"] = keyAttributes;
            AggregatedResultTestSetup.SetTextAggregatedResultsInDb(TestHelper.UnitOfWork,
                maintainableAssets, resultAttributes, "ok");
            AggregatedResultTestSetup.SetTextAggregatedResultsInDb(TestHelper.UnitOfWork,
                maintainableAssets, resultDictionary);

            var controller = CreateController();
            var fillModel = new CommittedProjectFillTreatmentValuesModel
            {
                TreatmentId = treatmentId,
                NetworkId = networkId,
                KeyAttributeValue = assetKeyData,
            };

            var result = await controller.FillTreatmentValues(fillModel);

            var value = ActionResultAssertions.OkObject(result);
            var castValue = value as CommittedProjectFillTreatmentReturnValuesModel;
            Assert.Equal(12345, castValue.TreatmentCost);
        }
    }
}
