using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Treatment
{
    public class TreatmentRuleOverrideCriterionEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid TreatmentRuleOverrideId { get; set; }

        public string TreatmentName { get; set; }

        public string CriterionExpression { get; set; }

        public virtual TreatmentRuleOverrideEntity TreatmentRuleOverride { get; set; }
    }
}
