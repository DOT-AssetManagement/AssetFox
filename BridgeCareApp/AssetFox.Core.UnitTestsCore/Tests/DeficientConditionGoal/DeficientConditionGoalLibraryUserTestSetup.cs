using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs.Enums;
using AssetFox.Core.DTOs;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;

namespace AssetFox.Core.UnitTestsCore.Tests.DeficientConditionGoal
{
    public static class DeficientConditionGoalLibraryUserTestSetup
    {
        public static LibraryUserDTO CreateLibraryUserDto(Guid userId, LibraryAccessLevel accessLevel = LibraryAccessLevel.Read)
        {
            var dto = new LibraryUserDTO
            {
                UserId = userId,
                AccessLevel = accessLevel,
            };
            return dto;
        }

        public static void SetUsersOfDeficientConditionGoalLibrary(
            IUnitOfWork unitOfWork,
            Guid deficientConditionGoalLibraryId,
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
                };
                dtos.Add(libraryUserDto);
            }
            unitOfWork.DeficientConditionGoalRepo.UpsertOrDeleteUsers(deficientConditionGoalLibraryId, dtos);
        }
    }
}
