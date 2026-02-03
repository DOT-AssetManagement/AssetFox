using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using Moq;

namespace AssetFoxCoreTests.Tests.AttributeDatum
{
    public static class AttributeDatumRepositoryMocks
    {
        public static Mock<IAttributeDatumRepository> New(Mock<IUnitOfWork> unitOfWork = null)
        {
            var mock = new Mock<IAttributeDatumRepository>();
            if (unitOfWork != null)
            {
                unitOfWork.Setup(u => u.AttributeDatumRepo).Returns(mock.Object);
            }
            return mock;
        }
    }
}
