using System;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.TestHelpers;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests.User
{
    public static class UserCriteriaDtos
    {
        public static UserCriteriaDTO Dto(Guid userId, string username, Guid? criteriaId = null)
        {
            var resolveCriteriaId = criteriaId ?? Guid.NewGuid();
            var criteriaName = RandomStrings.WithPrefix("Criteria");

            var dto = new UserCriteriaDTO
            {
                CriteriaId = resolveCriteriaId,
                UserId = userId,
                UserName = username,
                Name = criteriaName,
                HasCriteria = true,
                Criteria = "Criteria",
            };
            return dto;
        }
    }
}
