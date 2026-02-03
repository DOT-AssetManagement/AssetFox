using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.TargetConditionGoal
{
    public class CriterionLibraryScenarioTargetConditionGoalEntity : BaseEntity
    {
        public Guid ScenarioTargetConditionGoalId { get; set; }

        public Guid CriterionLibraryId { get; set; }

        public virtual ScenarioTargetConditionGoalEntity ScenarioTargetConditionGoal { get; set; }

        public virtual CriterionLibraryEntity CriterionLibrary { get; set; }
    }
}
