using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment
{
    public class ScenarioTreatmentRuleOverrideCriterionTreatmentEntity : BaseEntity
    {
        public Guid ScenarioTreatmentRuleOverrideCriterionId { get; set; }

        public Guid ScenarioSelectableTreatmentId { get; set; }

        public virtual ScenarioTreatmentRuleOverrideCriterionEntity ScenarioTreatmentRuleOverrideCriterion { get; set; }

        public virtual ScenarioSelectableTreatmentEntity ScenarioSelectableTreatment { get; set; }
    }
}
