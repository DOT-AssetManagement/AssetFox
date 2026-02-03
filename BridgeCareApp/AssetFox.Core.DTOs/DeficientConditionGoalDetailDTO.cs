using System;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class DeficientConditionGoalDetailDTO : BaseDTO
    {
        public Guid SimulationYearDetailId { get; set; }

        public double ActualDeficientPercentage { get; set; }

        public double AllowedDeficientPercentage { get; set; }

        public double DeficientLimit { get; set; }

        public Guid AttributeId { get; set; }

        public bool GoalIsMet { get; set; }

        public string GoalName { get; set; }
    }
}
