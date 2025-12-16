using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.BudgetPriority
{
    public class CriterionLibraryScenarioBudgetPriorityEntity : BaseCriterionLibraryJoinEntity
    {
        public Guid ScenarioBudgetPriorityId { get; set; }

        public virtual ScenarioBudgetPriorityEntity ScenarioBudgetPriority { get; set; }
    }
}
