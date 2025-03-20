using System;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class TargetConditionGoalDetailEntity: ConditionGoalDetailEntity
    {
        public Guid SimulationYearDetailId { get; set; }

        public virtual SimulationYearDetailEntity SimulationYearDetail { get; set; }

        public double ActualValue { get; set; }

        public double TargetValue { get; set; }
    }
}
