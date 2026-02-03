using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract
{
    public abstract class BaseCalculatedAttributeEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid AttributeId { get; set; }

        public int CalculationTiming { get; set; }

        public virtual AttributeEntity Attribute { get; set; }
    }
}
