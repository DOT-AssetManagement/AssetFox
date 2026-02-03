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
    public static class AttributeRepositoryMocks
    {
        public static Mock<IAttributeRepository> New(Mock<IUnitOfWork> mockUnitOfWork = null)
        {
            var mock = new Mock<IAttributeRepository>();
            if (mockUnitOfWork != null )
            {
                mockUnitOfWork.Setup(m => m.AttributeRepo).Returns(mock.Object);
            }
            return mock;
        }
    }
}
