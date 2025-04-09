using System;
using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class TargetConditionGoalDetailDTO : BaseDTO
    {
        public Guid SimulationYearDetailId { get; set; }

        public double ActualValue { get; set; }

        public double TargetValue { get; set; }
    }
}
