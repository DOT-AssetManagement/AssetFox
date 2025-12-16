using System;
using System.Collections.Generic;
using System.Text;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Deficient
{
    public class ScenarioDeficientConditionGoalEntity : ConditionGoalEntity
    {
        public Guid SimulationId { get; set; }

        public double AllowedDeficientPercentage { get; set; }

        public double DeficientLimit { get; set; }

        public Guid LibraryId { get; set; }

        public bool IsModified { get; set; }

        public virtual SimulationEntity Simulation { get; set; }

        public virtual CriterionLibraryScenarioDeficientConditionGoalEntity CriterionLibraryScenarioDeficientConditionGoalJoin { get; set; }
    }
}
