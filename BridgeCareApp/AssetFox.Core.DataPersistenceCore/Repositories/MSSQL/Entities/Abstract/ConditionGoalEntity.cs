using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract
{
    public abstract class ConditionGoalEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid AttributeId { get; set; }

        public virtual AttributeEntity Attribute { get; set; }
    }
}
