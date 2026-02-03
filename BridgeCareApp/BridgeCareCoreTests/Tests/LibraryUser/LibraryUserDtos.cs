using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Enums;

namespace AssetFoxCoreTests.Tests
{
    public static class LibraryUserDtos
    {
        public static LibraryUserDTO Modify(Guid userId, string userName="testLibraryUser")
        {
            var user = new LibraryUserDTO
            {
                UserId = userId,
                UserName = userName,
                AccessLevel = LibraryAccessLevel.Modify
            };
            return user;
        }
    }
}
