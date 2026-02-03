using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Deficient
{
    public class DeficientConditionGoalEntity : ConditionGoalEntity
    {
        public Guid DeficientConditionGoalLibraryId { get; set; }

        public double AllowedDeficientPercentage { get; set; }

        public double DeficientLimit { get; set; }

        public virtual DeficientConditionGoalLibraryEntity DeficientConditionGoalLibrary { get; set; }

        public virtual CriterionLibraryDeficientConditionGoalEntity CriterionLibraryDeficientConditionGoalJoin { get; set; }
    }
}
