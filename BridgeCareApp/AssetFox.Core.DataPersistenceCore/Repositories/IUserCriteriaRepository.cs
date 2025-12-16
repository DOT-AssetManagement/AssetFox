using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IUserCriteriaRepository
    {
        public UserCriteriaDTO GetOwnUserCriteria(UserInfoDTO userInfo);
        public List<UserCriteriaDTO> GetAllUserCriteria();
        public void UpsertUserCriteria(UserCriteriaDTO dto);
        public void DeactivateUser(Guid userId);
        public void ReactivateUser(Guid userId);
        public void RevokeUserAccess(Guid userCriteriaId);

        public string GetUserCriteria(Guid userId);
    }
}
