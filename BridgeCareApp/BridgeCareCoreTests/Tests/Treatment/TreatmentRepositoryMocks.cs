using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DataPersistenceCore;
using AssetFox.Core.DTOs.Enums;
using AssetFox.Core.DTOs;
using Moq;

namespace AssetFoxCoreTests.Tests.Treatment
{
    public static class TreatmentRepositoryMocks
    {
        public static Mock<ITreatmentLibraryUserRepository> New(Mock<IUnitOfWork> unitOfWork = null)
        {
            var mock = new Mock<ITreatmentLibraryUserRepository>();
            unitOfWork?.Setup(u => u.TreatmentLibraryUserRepo).Returns(mock.Object);
            return mock;
        }

        public static void SetupLibraryAccessLibraryDoesNotExist(this Mock<ITreatmentLibraryUserRepository> mock, Guid libraryId)
        {
            var libraryAccess = LibraryAccessModels.LibraryDoesNotExist;
            mock.Setup(m => m.GetLibraryAccess(libraryId, It.IsAny<Guid>())).Returns(libraryAccess);
        }

        ///<summary>Pass in null for the access level to tell the mock to return a LibraryAccessModel with no users.</summary>  
        public static void SetupGetLibraryAccess(this Mock<ITreatmentLibraryUserRepository> mock, Guid libraryId, Guid userId, LibraryAccessLevel? accessLevel)
        {
            LibraryUserDTO access = null;
            if (accessLevel != null)
            {
                access = new LibraryUserDTO
                {
                    AccessLevel = accessLevel.Value,
                    UserId = userId,
                };
            }
            var dto = new LibraryUserAccessModel
            {
                LibraryExists = true,
                UserId = userId,
                Access = access,
            };
            mock.Setup(m => m.GetLibraryAccess(libraryId, userId)).Returns(dto);
        }

        public static void SetupGetLibaryUsers(this Mock<ITreatmentLibraryUserRepository> repository, Guid libraryId, List<LibraryUserDTO> users)
        {
            repository.Setup(r => r.GetLibraryUsers(libraryId)).Returns(users);
        }
    }
}
