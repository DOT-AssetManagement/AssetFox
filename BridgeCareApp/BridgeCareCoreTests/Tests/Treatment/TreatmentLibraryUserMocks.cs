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
    public static class TreatmentLibraryUserMocks
    {
        public static Mock<ITreatmentLibraryUserRepository> New(Mock<IUnitOfWork> unitOfWork = null)
        {
            var mock = new Mock<ITreatmentLibraryUserRepository>();
            if (unitOfWork != null)
            {
                unitOfWork.Setup(u => u.TreatmentLibraryUserRepo).Returns(mock.Object);
            }
            return mock;
        }
        public static void SetupGetLibraryAccess(this Mock<ITreatmentLibraryUserRepository> mock, Guid libraryId, LibraryUserAccessModel accessModel)
        {
            mock.Setup(r => r.GetLibraryAccess(libraryId, It.IsAny<Guid>())).Returns(accessModel);
        }
    }
}
