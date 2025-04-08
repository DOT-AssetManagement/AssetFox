using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using Moq;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests
{
    public static class UserCriteriaRepositoryMocks
    {
        public static Mock<IUserCriteriaRepository> New(Mock<IUnitOfWork> mockUnitOfWork = null)
        {
            var mockUserRepo = new Mock<IUserCriteriaRepository>();
            mockUnitOfWork?.Setup(uow => uow.UserCriteriaRepo).Returns(mockUserRepo.Object);
            return mockUserRepo;
        }
    }
}
