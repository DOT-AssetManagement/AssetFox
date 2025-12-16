using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class AttributeDatumLocationEntity : LocationEntity
    {
        public AttributeDatumLocationEntity(Guid id, string discriminator, string locationIdentifier) : base(id,
            discriminator, locationIdentifier)
        { }

        public Guid AttributeDatumId { get; set; }

        public virtual AttributeDatumEntity AttributeDatum { get; set; }
    }
}
