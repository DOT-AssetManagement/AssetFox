using System;
using AssetFox.Core.DTOs;
using AssetFox.Core.UnitTestsCore.TestUtils;

namespace AssetFox.Core.UnitTestsCore.Tests.User
{
    public class UserCriteriaTestSetup
    {
        public static UserCriteriaDTO ModelForEntityInDb(Guid userId, string username, bool admin)
        {
            var dto = new UserCriteriaDTO
            {
                UserId = userId,
                UserName = username,
                HasAccess = admin,
            };
            TestHelper.UnitOfWork.UserCriteriaRepo.UpsertUserCriteria(dto);
            return dto;
        }
    }
}
