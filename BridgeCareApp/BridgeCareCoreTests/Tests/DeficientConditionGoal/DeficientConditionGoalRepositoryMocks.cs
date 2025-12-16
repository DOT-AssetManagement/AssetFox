using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using Moq;

namespace AssetFoxCoreTests.Tests
{
    public static class DeficientConditionGoalRepositoryMocks
    {
        public static Mock<IDeficientConditionGoalRepository> New(Mock<IUnitOfWork> unitOfWorkMock = null)
        {
            var repo = new Mock<IDeficientConditionGoalRepository>();
            if (unitOfWorkMock != null)
            {
                unitOfWorkMock.Setup(u => u.DeficientConditionGoalRepo).Returns(repo.Object);
            }
            return repo;
        }
        public static void SetupGetLibraryAccess(this Mock<IDeficientConditionGoalRepository> mock, Guid libraryId, LibraryUserAccessModel accessModel)
        {
            mock.Setup(r => r.GetLibraryAccess(libraryId, It.IsAny<Guid>())).Returns(accessModel);
        }
    }
}
