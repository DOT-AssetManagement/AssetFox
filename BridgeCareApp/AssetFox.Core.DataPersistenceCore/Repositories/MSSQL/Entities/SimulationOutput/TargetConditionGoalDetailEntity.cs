using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class TargetConditionGoalDetailEntity: ConditionGoalDetailEntity
    {
        public Guid SimulationYearDetailId { get; set; }

        public int RunId { get; set; } // Is this intended to hide RunId in the superclass?

        public virtual SimulationYearDetailEntity SimulationYearDetail { get; set; }

        public double ActualValue { get; set; }

        public double TargetValue { get; set; }
    }
}
