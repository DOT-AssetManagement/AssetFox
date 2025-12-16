using AssetFox.Core.DTOs.Abstract;
using System;
using System.Collections.Generic;

namespace AssetFox.Core.DTOs
{
    public class UserDTO : BaseDTO
    {
        public string Username { get; set; }

        public bool ActiveStatus { get; set; } 

        public bool HasInventoryAccess { get; set; }

        public DateTime LastNewsAccessDate { get; set; }

        public CriterionLibraryDTO CriterionLibrary { get; set; }
    }
}

