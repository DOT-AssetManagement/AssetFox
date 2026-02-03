using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public abstract class ConditionGoalDetailEntity
    {
        public Guid Id { get; set; }

        public int RunId { get; set; }

        public virtual AttributeEntity Attribute { get; set; }

        public Guid AttributeId { get; set; }

        public bool GoalIsMet { get; set; }

        public string GoalName { get; set; }
    }
}
