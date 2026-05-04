using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class ReportSchemaUserEntity : BaseEntity
    {
        public Guid ReportSchemaId { get; set; }

        public Guid UserId { get; set; }

        public int AccessLevel { get; set; }

        public virtual ReportSchemaEntity ReportSchema { get; set; }

        public virtual UserEntity User { get; set; }
    }
}
