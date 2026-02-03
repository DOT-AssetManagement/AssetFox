using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class TargetConditionGoalDetailEntity: ConditionGoalDetailEntity
    {
        public Guid SimulationYearDetailId { get; set; }

        public int RunId { get; set; }

        public virtual SimulationYearDetailEntity SimulationYearDetail { get; set; }

        public double ActualValue { get; set; }

        public double TargetValue { get; set; }
    }
}
