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
    public class AnalysisMethodRepositoryMocks
    {
        public static Mock<IAnalysisMethodRepository> New(Mock<IUnitOfWork> unitOfWork = null)
        {
            var mock = new Mock<IAnalysisMethodRepository>();
            if (unitOfWork != null)
            {
                unitOfWork.Setup(u => u.AnalysisMethodRepo).Returns(mock.Object);
            }
            return mock;
        }
    }
}
