using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Treatment
{
    public class TreatmentRuleOverrideCriterionEntity : BaseEntity
    {
        public TreatmentRuleOverrideCriterionEntity()
        {
            TreatmentJoins = new HashSet<TreatmentRuleOverrideCriterionTreatmentEntity>();
        }

        public Guid Id { get; set; }

        public Guid TreatmentRuleOverrideId { get; set; }

        public string LogicalOperator { get; set; } = "AND";

        public int SortOrder { get; set; }

        public string CriterionExpression { get; set; }

        public virtual TreatmentRuleOverrideEntity TreatmentRuleOverride { get; set; }

        public virtual ICollection<TreatmentRuleOverrideCriterionTreatmentEntity> TreatmentJoins { get; set; }
    }
}
