using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.RemainingLifeLimit
{
    public class RemainingLifeLimitLibraryUserEntity : LibraryUserBaseEntity
    {
        public virtual RemainingLifeLimitLibraryEntity RemainingLifeLimitLibrary { get; set; }
    }
}
