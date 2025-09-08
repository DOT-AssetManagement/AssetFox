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

        public virtual AttributeEntity Attribute { get; set; }
    }
}

// TODO

// On upload of file to DS mapping table entries to be saved - done
// Irresepective of mappings worksheet presence in DS excel, all non calculated attributes mappings to be added
// (with value if column name - case sensitive or None)
// - Save (inserts) done...
//TOO Test
// 1. With no mappings provided in raw excel -- Done
// 2. Edit/update from UI

// Edit mappings in DS UI - TODO complete it -- Read from DB(new controller and UI side service n module)? and show in UI, then make it editable - test upsert!
