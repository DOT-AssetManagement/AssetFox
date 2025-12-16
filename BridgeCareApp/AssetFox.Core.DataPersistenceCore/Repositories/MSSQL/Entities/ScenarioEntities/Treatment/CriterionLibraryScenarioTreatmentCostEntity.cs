using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment
{
    public class CriterionLibraryScenarioTreatmentCostEntity : BaseCriterionLibraryJoinEntity
    {
        public Guid ScenarioTreatmentCostId { get; set; }
        public virtual ScenarioTreatmentCostEntity ScenarioTreatmentCost { get; set; }
    }
}
