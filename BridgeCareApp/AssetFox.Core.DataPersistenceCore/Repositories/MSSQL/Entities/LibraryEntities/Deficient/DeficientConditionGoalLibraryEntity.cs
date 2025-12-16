using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Deficient
{
    public class DeficientConditionGoalLibraryEntity : LibraryEntity
    {
        public DeficientConditionGoalLibraryEntity()
        {
            DeficientConditionGoals = new HashSet<DeficientConditionGoalEntity>();
            Users = new HashSet<DeficientConditionGoalLibraryUserEntity>();
        }

        public virtual ICollection<DeficientConditionGoalEntity> DeficientConditionGoals { get; set; }
        public virtual ICollection<DeficientConditionGoalLibraryUserEntity> Users { get; set; }
    }
}
