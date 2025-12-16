using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment
{
    public class ScenarioTreatmentCostEquationEntity : BaseEquationJoinEntity
    {
        public Guid ScenarioTreatmentCostId { get; set; }
        public virtual ScenarioTreatmentCostEntity ScenarioTreatmentCost { get; set; }
    }
}
