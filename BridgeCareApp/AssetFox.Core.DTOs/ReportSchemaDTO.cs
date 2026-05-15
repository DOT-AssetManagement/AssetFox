using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;
using AssetFox.Core.DTOs.Enums;

namespace AssetFox.Core.DTOs
{
    public class ReportSchemaDTO : BaseDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string SchemaJson { get; set; }
        public string ReportCategory { get; set; }
        public bool IsSystemDefault { get; set; }
        public bool IsSharedWithAll { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string OwnerName { get; set; }
        public LibraryAccessLevel? CurrentUserAccessLevel { get; set; }
        public bool CanModify { get; set; }
        public bool CanShare { get; set; }
        public bool IsShared { get; set; }
        public int SharedUserCount { get; set; }
        public int PageCount { get; set; }
        public int ComponentCount { get; set; }
        public List<LibraryUserDTO> Users { get; set; } = new List<LibraryUserDTO>();
    }
}
