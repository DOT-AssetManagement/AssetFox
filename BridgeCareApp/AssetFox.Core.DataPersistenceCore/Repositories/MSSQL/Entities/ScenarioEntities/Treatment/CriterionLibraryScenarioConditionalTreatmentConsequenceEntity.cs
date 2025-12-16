using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment
{
    public class CriterionLibraryScenarioConditionalTreatmentConsequenceEntity : BaseCriterionLibraryJoinEntity
    {
        public Guid ScenarioConditionalTreatmentConsequenceId { get; set; }

        public virtual ScenarioConditionalTreatmentConsequenceEntity ScenarioConditionalTreatmentConsequence { get; set; }
    }
}
