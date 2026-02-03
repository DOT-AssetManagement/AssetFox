using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment
{
    public class CriterionLibraryScenarioSelectableTreatmentEntity : BaseCriterionLibraryJoinEntity
    {
        public Guid ScenarioSelectableTreatmentId { get; set; }

        public virtual ScenarioSelectableTreatmentEntity ScenarioSelectableTreatment { get; set; }
    }
}
