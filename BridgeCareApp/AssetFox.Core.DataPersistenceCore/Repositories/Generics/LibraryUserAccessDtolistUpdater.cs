using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Enums;

namespace AssetFox.Core.DataPersistenceCore.Repositories.Generics
{
    public static class LibraryUserDtolists
    {
        public static List<LibraryUserDTO> OwnerAccess(Guid userId)
        {

            var owner = new LibraryUserDTO
            {
                AccessLevel = LibraryAccessLevel.Owner,
                UserId = userId,
            };
            var userList = new List<LibraryUserDTO> { owner };
            return userList;
        }
    }
}
