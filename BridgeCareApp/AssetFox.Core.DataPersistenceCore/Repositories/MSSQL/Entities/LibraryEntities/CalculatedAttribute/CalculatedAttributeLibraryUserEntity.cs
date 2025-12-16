using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.CalculatedAttribute
{
    public class CalculatedAttributeLibraryUserEntity : LibraryUserBaseEntity
    {
        public virtual CalculatedAttributeLibraryEntity CalculatedAttributeLibrary { get; set; }
    }
}
