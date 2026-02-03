using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.UnitTestsCore.Tests;
using Moq;

namespace AssetFox.Core.UnitTestsCore.TestUtils
{
    public static class UnitOfWorkMocks
    {
        public static Mock<IUnitOfWork> New()
        {
            var unitOfWork = new Mock<IUnitOfWork>();
            return unitOfWork;
        }
        public static Mock<IUnitOfWork> EveryoneExists()
        {
            var mock = New();
            UserRepositoryMocks.EveryoneExists(mock);
            return mock;
        }
        public static Mock<IUnitOfWork> WithCurrentUser(UserDTO user)
        {
            var unitOfWork = New();
            unitOfWork.Setup(u => u.CurrentUser).Returns(user);
            var userRepository = UserRepositoryMocks.UserExists(user.Username);
            unitOfWork.Setup(u => u.UserRepo).Returns(userRepository.Object);
            return unitOfWork;
        }
    }
}
