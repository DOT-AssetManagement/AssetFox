using System;
using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class DeficientConditionGoalDetailDTO : BaseDTO
    {
        public Guid SimulationYearDetailId { get; set; }

        public double ActualDeficientPercentage { get; set; }

        public double AllowedDeficientPercentage { get; set; }

        public double DeficientLimit { get; set; }
    }
}
