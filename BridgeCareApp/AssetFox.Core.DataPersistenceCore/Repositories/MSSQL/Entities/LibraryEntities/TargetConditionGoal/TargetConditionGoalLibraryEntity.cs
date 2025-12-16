using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Budget;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.TargetConditionGoal
{
    public class TargetConditionGoalLibraryEntity : LibraryEntity
    {
        public TargetConditionGoalLibraryEntity()
        {
            TargetConditionGoals = new HashSet<TargetConditionGoalEntity>();
            Users = new HashSet<TargetConditionGoalLibraryUserEntity>();
        }

        public virtual ICollection<TargetConditionGoalEntity> TargetConditionGoals { get; set; }
        public virtual ICollection<TargetConditionGoalLibraryUserEntity> Users { get; set; }
    }
}
