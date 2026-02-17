using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class ReportSchemaEntity : BaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SchemaJson { get; set; }
        public string ReportCategory { get; set; }
        public bool IsSystemDefault { get; set; }
    }
}
