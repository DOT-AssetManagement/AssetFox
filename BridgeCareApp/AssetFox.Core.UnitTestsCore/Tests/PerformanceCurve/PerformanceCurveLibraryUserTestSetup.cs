using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs.Enums;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class PerformanceCurveLibraryUserTestSetup
    {
        public static void SetUsersOfPerformanceCurveLibrary(
            IUnitOfWork unitOfWork,
            Guid performanceCurveLibraryId,
            LibraryAccessLevel accessLevelForAllListedUsers,
            params Guid[] userIds)
        {
            var dtos = new List<LibraryUserDTO>();
            foreach (var userId in userIds)
            {
                var libraryUserDto = new LibraryUserDTO
                {
                    AccessLevel = accessLevelForAllListedUsers,
                    UserId = userId,
                    UserName = "Test User"
                };
                dtos.Add(libraryUserDto);
            }
            unitOfWork.PerformanceCurveRepo.UpsertOrDeleteUsers(performanceCurveLibraryId, dtos);
        }
    }
}
