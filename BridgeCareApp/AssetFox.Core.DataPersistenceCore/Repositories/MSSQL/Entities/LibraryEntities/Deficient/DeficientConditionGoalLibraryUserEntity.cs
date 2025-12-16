using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.RemainingLifeLimit;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Deficient
{
    public class DeficientConditionGoalLibraryUserEntity : LibraryUserBaseEntity
    {
        public virtual DeficientConditionGoalLibraryEntity DeficientConditionGoalLibrary { get; set; }
    }
}
