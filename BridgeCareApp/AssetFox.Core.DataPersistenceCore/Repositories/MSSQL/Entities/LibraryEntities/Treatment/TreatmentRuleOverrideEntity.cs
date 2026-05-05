using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Treatment
{
    public class TreatmentRuleOverrideEntity : BaseEntity
    {
        public TreatmentRuleOverrideEntity()
        {
            Criteria = new HashSet<TreatmentRuleOverrideCriterionEntity>();
        }

        public Guid Id { get; set; }

        public Guid TreatmentLibraryId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsEnabledByDefault { get; set; } = true;

        public virtual TreatmentLibraryEntity TreatmentLibrary { get; set; }

        public virtual ICollection<TreatmentRuleOverrideCriterionEntity> Criteria { get; set; }
    }
}
