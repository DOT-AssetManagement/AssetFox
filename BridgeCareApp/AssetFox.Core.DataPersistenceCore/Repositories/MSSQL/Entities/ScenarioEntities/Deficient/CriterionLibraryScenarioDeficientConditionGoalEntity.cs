using System;
using System.Collections.Generic;
using System.Text;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Deficient
{
    public class CriterionLibraryScenarioDeficientConditionGoalEntity : BaseEntity
    {
        public Guid CriterionLibraryId { get; set; }

        public Guid ScenarioDeficientConditionGoalId { get; set; }

        public virtual CriterionLibraryEntity CriterionLibrary { get; set; }

        public virtual ScenarioDeficientConditionGoalEntity ScenarioDeficientConditionGoal { get; set; }
    }
}
