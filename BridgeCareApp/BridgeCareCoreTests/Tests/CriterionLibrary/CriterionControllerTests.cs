using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.UnitTestsCore.Extensions;
using AssetFox.Core.UnitTestsCore.Tests;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Controllers;
using AssetFoxCore.Services;
using AssetFoxCoreTests.Helpers;
using Moq;
using Xunit;

namespace AssetFoxCoreTests.Tests
{
    public class CriterionLibraryControllerTests
    {

        private CriterionLibraryController CreateController(Mock<IUnitOfWork> unitOfWork)
        {
            var service = new CashFlowPagingService(unitOfWork.Object);
            var security = EsecSecurityMocks.AdminMock;
            var hubService = HubServiceMocks.New();
            var accessor = HttpContextAccessorMocks.DefaultMock();
            var claimHelper = ClaimHelperMocks.New();
            var controller = new CriterionLibraryController(
                security.Object,
                unitOfWork.Object,
                hubService.Object,
                accessor.Object
                );
            return controller;
        }

        [Fact]
        public async Task CriterionLibraries_CallsThroughToRepo()
        {
            var unitOfWork = UnitOfWorkMocks.New();
            UserRepositoryMocks.EveryoneExists(unitOfWork);
            var criterionLibraryRepo = CriterionLibraryRepositoryMocks.New(unitOfWork);
            var controller = CreateController(unitOfWork);

            // Act
            var result = await controller.CriterionLibraries();

            // Assert
            ActionResultAssertions.OkObject(result);
            criterionLibraryRepo.SingleInvocationWithName(nameof(ICriterionLibraryRepository.CriterionLibraries));
        }

        [Fact]
        public async Task UpsertCriterionLibrary_CallsThroughToRepo()
        {
            var unitOfWork = UnitOfWorkMocks.New();
            UserRepositoryMocks.EveryoneExists(unitOfWork);
            var repo = CriterionLibraryRepositoryMocks.New(unitOfWork);
            var controller = CreateController(unitOfWork);
            var dto = CriterionLibraryTestSetup.TestCriterionLibrary();

            // Act
            var result = await controller
                .UpsertCriterionLibrary(dto);

            // Assert
            ActionResultAssertions.OkObject(result);
            var invocation = repo.SingleInvocationWithName(nameof(ICriterionLibraryRepository.UpsertCriterionLibrary));
            Assert.Equal(dto, invocation.Arguments[0]);
        }

        [Fact]
        public async Task DeleteCriterionLibrary_CallsThroughToRepo()
        {
            var unitOfWork = UnitOfWorkMocks.New();
            UserRepositoryMocks.EveryoneExists(unitOfWork);
            var repo = CriterionLibraryRepositoryMocks.New(unitOfWork);
            var controller = CreateController(unitOfWork);
            var libraryId = Guid.NewGuid();
            // Act
            var result = await controller.DeleteCriterionLibrary(libraryId);

            // Assert
            ActionResultAssertions.Ok(result);
            var invocation = repo.SingleInvocationWithName(nameof(ICriterionLibraryRepository.DeleteCriterionLibrary));
            Assert.Equal(libraryId, invocation.Arguments[0]);
        }
    }
}
