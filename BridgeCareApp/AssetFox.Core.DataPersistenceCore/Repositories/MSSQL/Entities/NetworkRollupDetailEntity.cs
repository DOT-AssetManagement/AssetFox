using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class NetworkRollupDetailEntity : BaseEntity
    {
        public Guid NetworkId { get; set; }

        public string Status { get; set; }

        public virtual NetworkEntity Network { get; set; }
    }
}
