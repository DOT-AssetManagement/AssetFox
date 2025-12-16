using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract
{
    public abstract class TreatmentConsequenceEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid AttributeId { get; set; }

        public string ChangeValue { get; set; }

        public virtual AttributeEntity Attribute { get; set; }
    }
}
