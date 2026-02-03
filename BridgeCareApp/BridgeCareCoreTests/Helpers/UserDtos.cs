using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;
using AssetFox.Core.UnitTestsCore.TestUtils;

namespace AssetFoxCoreTests.Helpers
{
    public static class UserDtos
    {
        public static UserDTO Admin() => new UserDTO
        {
            Username = "Admin",
            HasInventoryAccess = true,
            Id = TestDataForCommittedProjects.AuthorizedUser
        };

        public static UserDTO Dbe(Guid? id = null)
        {
            var resolveId = id ?? Guid.NewGuid();
            var dto = new UserDTO
            {
                Username = "Dbe",
                HasInventoryAccess = true,
                Id = resolveId,
            };
            return dto;
        }
    }
}
