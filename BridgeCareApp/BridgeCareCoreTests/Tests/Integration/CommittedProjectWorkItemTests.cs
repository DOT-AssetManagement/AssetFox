using AppliedResearchAssociates.iAM.DataUnitTests.Tests;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Repositories;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using BridgeCareCoreTests.Helpers;
using Xunit;
using BridgeCareCore.Controllers;
using BridgeCareCore.Services.SummaryReport.CommittedProjects;
using BridgeCareCore.Services;
using BridgeCareCoreTests.Tests.General_Work_Queue;
using AppliedResearchAssociates.iAM.Data.Networking;
using BridgeCareCore.Services.General_Work_Queue.WorkItems;
using OfficeOpenXml;

namespace BridgeCareCoreTests.Tests.Integration
{
    public class CommittedProjectWorkItemTests
    {
        [Fact]
        public void ExportCommittedProjects_ThenImport_Expected_WorkItemLevelVersion()
        {
            // WJPRQ This is an alternate version of the test ExportCommittedProjects_ThenImport_Expected.
            // The other version uses the queues. It therefore tests more code but is more complicated.
            // This version has the drawback that it duplicates some controller code.
            // Which do we prefer?
            var hubService = HubServiceMocks.Default();
            var service = new CommittedProjectService(TestHelper.UnitOfWork, hubService);
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            AdminSettingsTestSetup.SetupBamsAdminSettingsForTestNetwork(TestHelper.UnitOfWork, true);
            var simulationId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
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
            var committedProject = CommittedProjectTestSetup.ModelForEntityInDb(scenarioBudgetId, simulationId, locationKey, locationValue, treatmentName);
            var committedProjectsBefore = TestHelper.UnitOfWork.CommittedProjectRepo.GetSectionCommittedProjectDTOs(simulationId);
            Assert.NotEmpty(committedProjectsBefore);

            var fileInfo = service.ExportCommittedProjectsFile(simulationId);

            var formFile = FormFiles.FromFileInfo(fileInfo);
            var stream = formFile.OpenReadStream();
            var excelPackage = new ExcelPackage(stream); // WJPRQ controller code
            var username = TestUsernames.Admin;
            var workItem = new ImportCommittedProjectWorkItem(
                simulationId, excelPackage, fileInfo.FileName,
                username, simulation.Name);  // WJPRQ controller code
            var serviceProvider = ServiceProviders.AdminControllersWithSimulationIdAndFiles(simulationId, formFile);
            var cancellationToken = new CancellationToken();
            workItem.DoWork(serviceProvider, (string str) => { }, cancellationToken);

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
    }
}
