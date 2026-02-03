using AssetFox.Core.DataPersistenceCore;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.Tests;
using AssetFox.Core.UnitTestsCore.Tests.DeficientConditionGoal;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Controllers;
using AssetFoxCore.Models;
using AssetFoxCore.Services;
using Microsoft.Data.SqlClient;
using Moq;
using Xunit;

namespace AssetFoxCoreTests.Tests.Integration
{
    public class DeficientConditionGoalControllerIntegrationTests
    {
        private DeficientConditionGoalController CreateController(Mock<IHubService> hubServiceMock)
        {
            var security = EsecSecurityMocks.Admin;
            var contextAccessor = HttpContextAccessorMocks.Default();
            var claimHelper = ClaimHelperMocks.New();
            var service = new DeficientConditionGoalPagingService(TestHelper.UnitOfWork);
            var controller = new DeficientConditionGoalController(
                security,
                TestHelper.UnitOfWork,
                hubServiceMock.Object,
                contextAccessor,
                claimHelper.Object,
                service);
            return controller;
        }

        [Fact]
        public async Task UpsertDeficientConditionGoalLibraryAndGoals_SecondPartFails_NothingHappens()
        {
            var library = DeficientConditionGoalLibraryTestSetup.ModelForEntityInDb(
                TestHelper.UnitOfWork);
            var goalId = Guid.NewGuid();
            var criterionLibraryId = Guid.NewGuid();
            var nonexistentAttributeName = "nonexistentAttribute";
            var goal = DeficientConditionGoalDtos.DtoWithIdOnlyCriterionLibrary(goalId, nonexistentAttributeName);
            library.DeficientConditionGoals.Add(goal);
            library.Description = "Updated description";
            var syncModel = new PagingSyncModel<DeficientConditionGoalDTO>
            {
                AddedRows = new List<DeficientConditionGoalDTO> { goal },
            };

            var upsertRequest = new LibraryUpsertPagingRequestModel<DeficientConditionGoalLibraryDTO, DeficientConditionGoalDTO>
            {
                Library = library,
                IsNewLibrary = false,
                SyncModel = syncModel,
            };
            var hubService = HubServiceMocks.New();
            var controller = CreateController(hubService);

            await controller.UpsertDeficientConditionGoalLibrary(upsertRequest);

            var message = hubService.GetSingleThreeArgumentErrorMessage();
            Assert.Contains(ErrorMessageConstants.NoAttributeFoundHavingName, message);
            var libraryAfter = TestHelper.UnitOfWork.DeficientConditionGoalRepo
                .GetDeficientConditionGoalLibrariesWithDeficientConditionGoals()
                .Single(lib => lib.Id == library.Id);
            Assert.Null(libraryAfter.Description);
        }

        [Fact]
        public async Task UpsertScenarioDeficientConditionGoals_ThrowsDuringLaterDbCall_NothingChanges()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var simulationId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
            var goalId = Guid.NewGuid();
            var goalId2 = Guid.NewGuid();
            var attributeName = TestAttributeNames.CulvDurationN;
            var goal = DeficientConditionGoalDtos.DtoWithIdOnlyCriterionLibrary(goalId, attributeName);
            var goal2 = DeficientConditionGoalDtos.DtoWithIdOnlyCriterionLibrary(goalId2, attributeName);
            var goals = new List<DeficientConditionGoalDTO> { goal, goal2 };
            TestHelper.UnitOfWork.DeficientConditionGoalRepo.UpsertOrDeleteScenarioDeficientConditionGoals(
                goals, simulationId);
            var criterionLibrary = CriterionLibraryTestSetup.TestCriterionLibrary();
            var localScenarioDeficientGoals = TestHelper.UnitOfWork.DeficientConditionGoalRepo.GetScenarioDeficientConditionGoals(simulationId);
            var goalIndexToDelete = localScenarioDeficientGoals.FindIndex(g => g.Id == goalId2);
            var goalToUpdate = localScenarioDeficientGoals.First();
            var updatedGoalId = goalToUpdate.Id;
            var deleteGoalId = localScenarioDeficientGoals[1].Id;
            goalToUpdate.Name = "Updated";
            goalToUpdate.CriterionLibrary = criterionLibrary;
            goalToUpdate.DeficientLimit = double.NaN;
            var goalsBefore = TestHelper.UnitOfWork.DeficientConditionGoalRepo.GetScenarioDeficientConditionGoals(simulationId);
            var hubServiceMock = HubServiceMocks.New();
            var controller = CreateController(hubServiceMock);
            var pagingSync = new PagingSyncModel<DeficientConditionGoalDTO>
            {
                UpdateRows = new List<DeficientConditionGoalDTO> { goalToUpdate },
                RowsForDeletion = new List<Guid> { deleteGoalId },
            };

            // Act
            await controller.UpsertScenarioDeficientConditionGoals(simulationId, pagingSync);

            // Assert
            var _ = hubServiceMock.GetSingleThreeArgumentErrorMessage();
            var goalsAfter = TestHelper.UnitOfWork.DeficientConditionGoalRepo.GetScenarioDeficientConditionGoals(simulationId);
            ObjectAssertions.Equivalent(goalsBefore, goalsAfter);
            Assert.Equal(2, goalsAfter.Count);
        }
    }
}
