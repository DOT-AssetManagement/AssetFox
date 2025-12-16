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
    public static class ExcelRawDataRepositoryMocks
    {
        public static Mock<IExcelRawDataRepository> New(Mock<IUnitOfWork> unitOfWork = null)
        {
            var mock = new Mock<IExcelRawDataRepository>();
            if (unitOfWork != null)
            {
                unitOfWork.Setup(u => u.ExcelWorksheetRepository).Returns(mock.Object);
            }
            return mock;
        }
    }
}
