using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class ExcelRawDataEntity: BaseEntity
    {
        public Guid Id { get; set; }
        public string SerializedContent { get; set; }
        public Guid DataSourceId { get; set; }
        public virtual DataSourceEntity DataSource { get; set; }
    }
}
