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
    public static class CashFlowRuleRepositoryMocks
    {
        public static Mock<ICashFlowRuleRepository> New(Mock<IUnitOfWork> unitOfWork = null)
        {
            var mock = new Mock<ICashFlowRuleRepository>();
            if (unitOfWork != null)
            {
                unitOfWork.Setup(u => u.CashFlowRuleRepo).Returns(mock.Object);
            }
            return mock;
        }
        public static void SetupGetLibraryAccess(this Mock<ICashFlowRuleRepository> mock, Guid libraryId, LibraryUserAccessModel accessModel)
        {
            mock.Setup(r => r.GetLibraryAccess(libraryId, It.IsAny<Guid>())).Returns(accessModel);
        }
    }
}
