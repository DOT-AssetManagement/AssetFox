using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Budget;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.TargetConditionGoal
{
    public class TargetConditionGoalLibraryUserEntity : LibraryUserBaseEntity
    {
        public virtual TargetConditionGoalLibraryEntity TargetConditionGoalLibrary { get; set; }
    }
}
