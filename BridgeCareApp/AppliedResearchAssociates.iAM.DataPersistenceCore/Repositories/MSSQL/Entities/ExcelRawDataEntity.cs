using System;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class ExcelRawDataEntity: BaseEntity
    {
        public Guid Id { get; set; }
        public string SerializedContent { get; set; }
        public Guid DataSourceId { get; set; }
        public virtual DataSourceEntity DataSource { get; set; }
    }
}
