using AssetFox.Core.Data.Networking;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL;
using AssetFox.Core.DataUnitTests;
using AssetFox.Core.DataUnitTests.Tests;
using AssetFox.Core.UnitTestsCore.Tests;
using AssetFox.Core.UnitTestsCore.Tests.Attributes;
using AssetFox.Core.UnitTestsCore.Tests.Attributes.CalculatedAttributes;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFox.Core.WorkQueue;
using AssetFoxCore.Controllers;
using AssetFoxCore.Models;
using AssetFoxCoreTests.Helpers;
using AssetFoxCoreTests.Tests.Integration;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace AssetFoxCoreTests.Tests.CommittedProjects
{
    public class CommittedProjectControllerWorkQueueTests
    {
        [Fact]
        public async Task ImportWorksWithValidData()
        {
            // Arrange
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var networkId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var keyAttributeName = TestAttributeNames.BrKey;
            var asset = MaintainableAssets.InNetwork(networkId, keyAttributeName, assetId);
            var assets = new List<MaintainableAsset> { asset };
            var network = NetworkTestSetup.ModelForEntityInDbWithExistingKeyAttribute(TestHelper.UnitOfWork, assets, TestAttributeIds.BrKeyId, networkId);
            CalculatedAttributeTestSetup.CreateDefaultCalculatedAttributeLibrary(TestHelper.UnitOfWork);

            var formFile = FormFileFromGoodData();
            var simulationId = Guid.NewGuid();
            var simulationDto = SimulationDtos.Dto(simulationId, "test simulation");
            TestHelper.UnitOfWork.SimulationRepo.CreateSimulation(networkId, simulationDto);
            var analysisMethod = TestHelper.UnitOfWork.AnalysisMethodRepo.GetAnalysisMethod(simulationId);
            var investmentPlanDto = InvestmentPlanDtos.Dto(simulationId, 2025, 1);
            TestHelper.UnitOfWork.InvestmentPlanRepo.UpsertInvestmentPlan(investmentPlanDto, simulationId);
            var keyAttributeNames = $"{TestAttributeNames.BrKey},{TestAttributeNames.BmsId}";
            AdminSettingsTestSetup.SetupBamsAdminSettings(TestHelper.UnitOfWork, network.Name, keyAttributeNames, keyAttributeNames);
            var serviceProvider = ServiceProviders.AdminControllersWithSimulationIdAndFiles(simulationId, formFile);
            var controller = CreateController(serviceProvider);

            // act 1
            var importResult = await controller.ImportCommittedProjects();
            ActionResultAssertions.Ok(importResult);

            // act 2
            var workStarter = await serviceProvider.DequeueAndCompleteHiddenUploadQueueTask();

            // Assert
            var castWorkStarter = workStarter as IQueuedWorkHandle<WorkQueueMetadata>;
            Assert.Equal(TaskStatus.RanToCompletion, castWorkStarter.WorkCompletion.Status);
        }

        [Fact]
        public async Task ImportFailsOnNoSimulation()
        {
            var formFile = FormFileFromGoodData();
            var simulationId = Guid.NewGuid();
            var serviceProvider = ServiceProviders.AdminControllersWithSimulationIdAndFiles(simulationId, formFile);
            var controller = CreateController(serviceProvider);

            // act 1
            var importResult = await controller.ImportCommittedProjects();
            ActionResultAssertions.Ok(importResult);

            // act 2
            var workStarter = await serviceProvider.DequeueAndCompleteHiddenUploadQueueTask();

            // Assert
            var castWorkStarter = workStarter as IQueuedWorkHandle<WorkQueueMetadata>;
            Assert.Equal(TaskStatus.Faulted, castWorkStarter.WorkCompletion.Status);
            var exception = castWorkStarter.WorkCompletion.Exception;
            var innerException = exception.InnerException;
            var message = innerException.Message;
            Assert.Equal(SimulationRepository.NoSimulationWasFoundForTheGivenScenario, message);
        }

        private CommittedProjectController CreateController(
            IServiceProvider serviceProvider
            )
        {
            var controller = serviceProvider.GetControllerWithUnifiedHttpContext<CommittedProjectController>();
            return controller;
        }

        private static FormFile FormFileFromGoodData()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestUtils\\Files",
                "TestCommittedProjects_Good.xlsx");
            var stream = File.OpenRead(filePath);
            var memStream = new MemoryStream();
            stream.CopyTo(memStream);
            var formFile = new FormFile(memStream, 0, memStream.Length, null, "TestCommittedProjects_Good.xlsx");
            return formFile;
        }

    }
}
