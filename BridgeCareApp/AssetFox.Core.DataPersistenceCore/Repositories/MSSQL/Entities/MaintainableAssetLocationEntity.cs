using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class MaintainableAssetLocationEntity : LocationEntity
    {
        public MaintainableAssetLocationEntity() {}

        public MaintainableAssetLocationEntity(Guid id, string discriminator, string locationIdentifier) : base(id,
            discriminator, locationIdentifier)
        { }

        public Guid MaintainableAssetId { get; set; }

        public virtual MaintainableAssetEntity MaintainableAsset { get; set; }
    }
}
