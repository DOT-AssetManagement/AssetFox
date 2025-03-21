using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.TestHelpers;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests.User
{
    public static class UserTestSetup
    {
        public static string NameForEntityInDb(IUnitOfWork unitOfWork, bool isAdmin, string? username = null)
        {
            var resolveUsername = username ?? RandomStrings.WithPrefix("user");
            unitOfWork.UserRepo.AddUser(resolveUsername, isAdmin);
            return resolveUsername;
        }

        public static async Task<UserDTO> ModelForEntityInDb(IUnitOfWork unitOfWork, bool isAdmin = false, string username = null)
        {
            var resolveUsername = NameForEntityInDb(unitOfWork, isAdmin, username);
            var userDto = await unitOfWork.UserRepo.GetUserByUserName(resolveUsername);
            return userDto;
        }
    }
}
