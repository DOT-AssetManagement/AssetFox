using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract
{
    public abstract class BasePerformanceCurveEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid AttributeId { get; set; }

        public string Name { get; set; }

        public bool Shift { get; set; }

        public virtual AttributeEntity Attribute { get; set; }
    }
}
