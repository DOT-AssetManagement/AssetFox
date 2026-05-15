using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Treatment
{
    public class TreatmentRuleOverrideCriterionTreatmentEntity : BaseEntity
    {
        public Guid TreatmentRuleOverrideCriterionId { get; set; }

        public Guid SelectableTreatmentId { get; set; }

        public virtual TreatmentRuleOverrideCriterionEntity TreatmentRuleOverrideCriterion { get; set; }

        public virtual SelectableTreatmentEntity SelectableTreatment { get; set; }
    }
}
