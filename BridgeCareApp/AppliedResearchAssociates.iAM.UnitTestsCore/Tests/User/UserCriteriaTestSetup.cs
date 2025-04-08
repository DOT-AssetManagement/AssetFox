using System;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests.User
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
