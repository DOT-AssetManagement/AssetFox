using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Budget;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class ScenarioSelectableTreatmentScenarioBudgetEntity : BaseEntity
    {
        public Guid ScenarioSelectableTreatmentId { get; set; }

        public Guid ScenarioBudgetId { get; set; }

        public virtual ScenarioBudgetEntity ScenarioBudget { get; set; }

        public virtual ScenarioSelectableTreatmentEntity ScenarioSelectableTreatment { get; set; }
    }
}
