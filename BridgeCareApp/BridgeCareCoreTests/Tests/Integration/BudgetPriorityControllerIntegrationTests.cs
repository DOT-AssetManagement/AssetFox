using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.Tests;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Controllers;
using AssetFoxCore.Models;
using AssetFoxCore.Services;
using AssetFoxCoreTests.Tests.BudgetPriority;
using Microsoft.Data.SqlClient;
using Moq;
using Xunit;

namespace AssetFoxCoreTests.Tests.Integration
{
    public class BudgetPriorityControllerIntegrationTests
    {
        public BudgetPriorityController CreateController(Mock<IHubService> hubServiceMock)
        {
            var security = EsecSecurityMocks.Admin;
            hubServiceMock ??= HubServiceMocks.New();
            var contextAccessor = HttpContextAccessorMocks.Default();
            var claimHelper = ClaimHelperMocks.New();
            var service = new BudgetPriorityPagingService(TestHelper.UnitOfWork);
            var controller = new BudgetPriorityController(
                security,
                TestHelper.UnitOfWork,
                hubServiceMock.Object,
                contextAccessor,
                claimHelper.Object,
                service
               );
            return controller;
        }

        [Fact]
        public async Task UpsertBudgetPriorityLibrary_ChildUpdateFails_NoChanges()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var libraryId = Guid.NewGuid();
            var library = BudgetPriorityLibraryDtos.New(libraryId);
            var library2 = BudgetPriorityLibraryDtos.New(libraryId);
            library2.Description = "Updated description";
            var priorityId = Guid.NewGuid();
            var childDto = BudgetPriorityDtos.New(priorityId);
            var childDto2 = BudgetPriorityDtos.New(priorityId);
            var budgetId = Guid.NewGuid();
            var budgetName = RandomStrings.WithPrefix("Budget");
            var criterionLibrary = CriterionLibraryDtos.Dto();
            criterionLibrary.Name = null;
            childDto.CriterionLibrary = criterionLibrary;
            TestHelper.UnitOfWork.BudgetPriorityRepo.UpsertBudgetPriorityLibrary(library);
            var budgetPriorities = new List<BudgetPriorityDTO> { childDto, childDto2 };
            var hubServiceMock = HubServiceMocks.New();

            var controller = CreateController(hubServiceMock);
            var upsertRequest = new LibraryUpsertPagingRequestModel<BudgetPriorityLibraryDTO, BudgetPriorityDTO>();
            upsertRequest.Library = library2;
            var syncModel = new PagingSyncModel<BudgetPriorityDTO> { AddedRows = budgetPriorities };
            upsertRequest.SyncModel = syncModel;

            await controller.UpsertBudgetPriorityLibrary(upsertRequest);

            var _ = hubServiceMock.GetSingleThreeArgumentErrorMessage();
            var librariesAfter = TestHelper.UnitOfWork.BudgetPriorityRepo.GetBudgetPriorityLibraries();
            var libraryAfter = librariesAfter.Single(
                lib => lib.Id == libraryId);
            Assert.Equal(library.Description, libraryAfter.Description);
        }
    }
}
