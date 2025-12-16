using System;
using System.Collections.Generic;
using System.Text;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public class BaseEntityProperties
    {
        public Guid CreatedBy { get; set; }
        public Guid LastModifiedBy { get; set;}

    }
}
