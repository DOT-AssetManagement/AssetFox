using System;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class DataSourceMappingEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public string DataField { get; set; } // column name or property from sql

        public Guid AttributeId { get; set; }

        public Guid DataSourceId { get; set; }

        public virtual DataSourceEntity DataSource { get; set; }
    }
}
// TODO

// On upload of file to DS mapping table entries to be saved
// (if no mapping excel given - it will add all columns and try find attributes (what if it doesn't find? - Check with Tyler)

// Edit mappings in DS UI
