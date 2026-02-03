using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Budget
{
    public class ScenarioBudgetAmountEntity : BaseBudgetAmountEntity
    {
        public Guid ScenarioBudgetId { get; set; }

        public virtual ScenarioBudgetEntity ScenarioBudget { get; set; }
    }
}
