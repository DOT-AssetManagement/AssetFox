using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class SectionLocationEntity : LocationEntity
    {
        public string UniqueIdentifier { get; set; }
    }
}
