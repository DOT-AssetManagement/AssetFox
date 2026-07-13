using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IUserRepository
    {
        void AddUser(string username, bool hasAdminClaim);

        /// <summary>
        ///     One-time migration of a user row that was keyed on the legacy (non-unique) display
        ///     name over to the new unique identifier. Renames the row in place, preserving its Id so
        ///     all scenarios and library shares (which reference User.Id) carry over. No-op if the old
        ///     row is missing or a row already exists under the new username.
        /// </summary>
        void MigrateUsername(string oldUsername, string newUsername, string displayName);

        void UpdateLastNewsAccessDate(Guid id, DateTime accessDate);

        Task<List<UserDTO>> GetAllUsers();

        Task<UserDTO> GetUserByUserName(string username);

        bool UserExists(string userName);

        Task<UserDTO> GetUserById(Guid id);
    }
}
