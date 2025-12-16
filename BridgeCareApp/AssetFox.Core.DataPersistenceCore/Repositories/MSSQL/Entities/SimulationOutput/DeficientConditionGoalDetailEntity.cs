using System;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class DeficientConditionGoalDetailEntity: ConditionGoalDetailEntity
    {
        public Guid SimulationYearDetailId { get; set; }

        public int RunId { get; set; }

        public virtual SimulationYearDetailEntity SimulationYearDetail { get; set; }

        public double ActualDeficientPercentage { get; set; }

        public double AllowedDeficientPercentage { get; set; }

        public double DeficientLimit { get; set; }
    }
}
