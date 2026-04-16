using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class DeficientConditionGoalDetailEntity: ConditionGoalDetailEntity
    {
        public Guid SimulationYearDetailId { get; set; }

        public int RunId { get; set; } // Is this intended to hide RunId in the superclass?

        public virtual SimulationYearDetailEntity SimulationYearDetail { get; set; }

        public double ActualDeficientPercentage { get; set; }

        public double AllowedDeficientPercentage { get; set; }

        public double DeficientLimit { get; set; }
    }
}
