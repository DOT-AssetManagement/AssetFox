using System;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;

namespace AssetFox.Core.UnitTestsCore.Tests.User
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
