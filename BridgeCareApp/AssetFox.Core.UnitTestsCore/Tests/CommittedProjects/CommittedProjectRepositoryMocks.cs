using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using Moq;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class CommittedProjectRepositoryMocks
    {
        public static Mock<ICommittedProjectRepository> New(Mock<IUnitOfWork> unitOfWork = null)
        {
            var mock = new Mock<ICommittedProjectRepository>();
            if (unitOfWork != null)
            {
                unitOfWork.Setup(u => u.CommittedProjectRepo).Returns(mock.Object);
            }
            return mock;
        }
    }
}
