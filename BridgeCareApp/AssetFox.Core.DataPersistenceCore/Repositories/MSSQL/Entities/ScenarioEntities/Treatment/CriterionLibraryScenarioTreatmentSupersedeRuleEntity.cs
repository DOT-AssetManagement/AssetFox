using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment
{
    public class CriterionLibraryScenarioTreatmentSupersedeRuleEntity : BaseCriterionLibraryJoinEntity
    {
        public Guid ScenarioTreatmentSupersedeRuleId { get; set; }

        public virtual ScenarioTreatmentSupersedeRuleEntity ScenarioTreatmentSupersedeRule { get; set; }
    }
}
