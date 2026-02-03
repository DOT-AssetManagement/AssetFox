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
    public static class TargetConditionGoalRepositoryMocks
    {
        public static Mock<ITargetConditionGoalRepository> New(Mock<IUnitOfWork> mockUnitOfWork = null)
        {
            var mock = new Mock<ITargetConditionGoalRepository>();
            if (mockUnitOfWork != null)
            {
                mockUnitOfWork.Setup(m => m.TargetConditionGoalRepo).Returns(mock.Object);
            }
            return mock;
        }
        public static void SetupGetLibraryAccess(this Mock<ITargetConditionGoalRepository> mock, Guid libraryId, LibraryUserAccessModel accessModel)
        {
            mock.Setup(r => r.GetLibraryAccess(libraryId, It.IsAny<Guid>())).Returns(accessModel);
        }
    }
}
