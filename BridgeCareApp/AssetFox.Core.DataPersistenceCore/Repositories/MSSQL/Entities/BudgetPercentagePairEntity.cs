using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Budget;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.BudgetPriority;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class BudgetPercentagePairEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid ScenarioBudgetId { get; set; }

        public Guid ScenarioBudgetPriorityId { get; set; }

        public decimal Percentage { get; set; }

        public virtual ScenarioBudgetEntity ScenarioBudget { get; set; }


        public virtual ScenarioBudgetPriorityEntity ScenarioBudgetPriority { get; set; }
    }
}
