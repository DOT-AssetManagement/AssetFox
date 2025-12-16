using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests;
using Moq;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils
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
