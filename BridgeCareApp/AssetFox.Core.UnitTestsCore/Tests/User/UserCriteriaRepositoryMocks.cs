using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using Moq;

namespace AssetFox.Core.UnitTestsCore.Tests
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
