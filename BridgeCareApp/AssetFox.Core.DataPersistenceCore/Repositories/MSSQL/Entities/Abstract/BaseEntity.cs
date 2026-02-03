using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract
{
    public abstract class BaseEntity
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime LastModifiedDate { get; set; } = DateTime.Now;

        // TODO: default these to empty guids for now until AssetFoxCore is integrated with authentication
        public Guid CreatedBy { get; set; } = Guid.Empty;

        public Guid LastModifiedBy { get; set; } = Guid.Empty;
    }
}
