using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Enums;
using Moq;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class PerformanceCurveRepositoryMocks
    {
        public static Mock<IPerformanceCurveRepository> New(Mock<IUnitOfWork> unitOfWork = null)
        {
            var mock = new Mock<IPerformanceCurveRepository>();
            if (unitOfWork!=null)
            {
                unitOfWork.Setup(u => u.PerformanceCurveRepo).Returns(mock.Object);
            }
            return mock;
        }

        ///<summary>Pass in null for the access level to tell the mock to return a LibraryAccessModel with no users.</summary>  
        public static void SetupGetLibraryAccess(this Mock<IPerformanceCurveRepository> mock, Guid libraryId, Guid userId, LibraryAccessLevel? accessLevel)
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

    }
}
