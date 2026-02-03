using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using Moq;

namespace AssetFoxCoreTests.Tests
{
    public static class MaintainableAssetRepositoryMocks
    {
        public static Mock<IMaintainableAssetRepository> New(Mock<IUnitOfWork> unitOfWork = null)
        {
            var mock = new Mock<IMaintainableAssetRepository>();
            if (unitOfWork != null)
            {
                unitOfWork.Setup(u => u.MaintainableAssetRepo).Returns(mock.Object);
            }
            return mock;
        }
    }
}
