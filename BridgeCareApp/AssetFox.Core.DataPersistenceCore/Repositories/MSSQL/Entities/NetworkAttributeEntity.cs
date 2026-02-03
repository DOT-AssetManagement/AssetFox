using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class NetworkAttributeEntity : BaseEntity
    {
        public Guid NetworkId { get; set; }
        public Guid AttributeId { get; set; }
        public virtual NetworkEntity Network { get; set; }
    }
}
