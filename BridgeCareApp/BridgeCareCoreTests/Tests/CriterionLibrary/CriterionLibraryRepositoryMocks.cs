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
    public static class CriterionLibraryRepositoryMocks
    {
        public static Mock<ICriterionLibraryRepository> New(Mock<IUnitOfWork> unitOfWork = null)
        {
            var repo = new Mock<ICriterionLibraryRepository>();
            if (unitOfWork != null)
            {
                unitOfWork.Setup(u => u.CriterionLibraryRepo).Returns(repo.Object);
            }
            return repo;
        }
    }
}
