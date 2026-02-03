using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.TargetConditionGoal
{
    public class CriterionLibraryTargetConditionGoalEntity : BaseEntity
    {
        public Guid TargetConditionGoalId { get; set; }

        public Guid CriterionLibraryId { get; set; }

        public virtual TargetConditionGoalEntity TargetConditionGoal { get; set; }

        public virtual CriterionLibraryEntity CriterionLibrary { get; set; }
    }
}
