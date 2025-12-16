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
    public static class CalculatedAttributeRepositoryMocks
    {
        public static Mock<ICalculatedAttributesRepository> New(Mock<IUnitOfWork> unitOfWork = null)
        {
            var mock = new Mock<ICalculatedAttributesRepository>();
            if (unitOfWork != null)
            {
                unitOfWork.Setup(u => u.CalculatedAttributeRepo).Returns(mock.Object);
            }
            return mock;
        }
    }
}
