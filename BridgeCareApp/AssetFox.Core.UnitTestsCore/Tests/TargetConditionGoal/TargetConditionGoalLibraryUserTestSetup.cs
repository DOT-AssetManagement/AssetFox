using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs.Enums;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests.TargetConditionGoal
{
    public class TargetConditionGoalLibraryUserTestSetup
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

        /// <summary>The targetConditionGoalLibrary and the user are both expected
        /// to exist before this method is called.</summary>  
        public static void SetUsersOfTargetConditionGoalLibrary(
            IUnitOfWork unitOfWork,
            Guid targetConditionGoalLibraryId,
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
            unitOfWork.TargetConditionGoalRepo.UpsertOrDeleteUsers(targetConditionGoalLibraryId, dtos);
        }
    }
}
