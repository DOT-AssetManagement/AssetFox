using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class DataSourceMappingEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public string DataField { get; set; } // column name or property from sql

        public Guid AttributeId { get; set; }

        public Guid DataSourceId { get; set; }

        public virtual DataSourceEntity DataSource { get; set; }

        public virtual AttributeEntity Attribute { get; set; }
    }
}
