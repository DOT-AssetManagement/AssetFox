using System;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class ReportSchemaDTO : BaseDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string SchemaJson { get; set; }
        public string ReportCategory { get; set; }
        public bool IsSystemDefault { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
