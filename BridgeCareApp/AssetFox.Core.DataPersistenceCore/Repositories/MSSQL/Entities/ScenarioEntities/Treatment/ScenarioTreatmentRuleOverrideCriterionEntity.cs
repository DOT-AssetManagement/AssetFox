using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment
{
    public class ScenarioTreatmentRuleOverrideCriterionEntity : BaseEntity
    {
        public ScenarioTreatmentRuleOverrideCriterionEntity()
        {
            TreatmentJoins = new HashSet<ScenarioTreatmentRuleOverrideCriterionTreatmentEntity>();
        }

        public Guid Id { get; set; }

        public Guid ScenarioTreatmentRuleOverrideId { get; set; }

        public string LogicalOperator { get; set; } = "AND";

        public int SortOrder { get; set; }

        public string CriterionExpression { get; set; }

        public virtual ScenarioTreatmentRuleOverrideEntity ScenarioTreatmentRuleOverride { get; set; }

        public virtual ICollection<ScenarioTreatmentRuleOverrideCriterionTreatmentEntity> TreatmentJoins { get; set; }
    }
}
