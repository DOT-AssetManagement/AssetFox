using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using Moq;

namespace AssetFoxCoreTests.Tests.Treatment
{
    public static class SelectableTreatmentMock
    {
        public static Mock<ISelectableTreatmentRepository> New(Mock<IUnitOfWork> unitOfWork = null)
        {
            var mock = new Mock<ISelectableTreatmentRepository>();
            if (unitOfWork != null)
            {
                unitOfWork.Setup(u => u.SelectableTreatmentRepo).Returns(mock.Object);
            }
            return mock;
        }
    }
}
