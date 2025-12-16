using System;
using AssetFox.Core.DataPersistenceCore.Migrations;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.TargetConditionGoal
{
    public class ScenarioTargetConditionGoalEntity : ConditionGoalEntity
    {
        public Guid SimulationId { get; set; }

        public double Target { get; set; }

        public int? Year { get; set; }

        public Guid LibraryId { get; set; }

        public bool IsModified { get; set; }

        public virtual SimulationEntity Simulation { get; set;}

        public virtual CriterionLibraryScenarioTargetConditionGoalEntity CriterionLibraryScenarioTargetConditionGoalJoin { get; set; }
    }
}
