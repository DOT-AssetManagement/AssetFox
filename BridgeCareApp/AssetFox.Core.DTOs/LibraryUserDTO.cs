using System;
using AssetFox.Core.DTOs.Enums;

namespace AssetFox.Core.DTOs
{
    public class LibraryUserDTO
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public LibraryAccessLevel AccessLevel { get; set; }
    }
}
