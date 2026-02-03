using System;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class TargetConditionGoalDetailDTO : BaseDTO
    {
        public double ActualValue { get; set; }

        public double TargetValue { get; set; }

        public Guid AttributeId { get; set; }

        public bool GoalIsMet { get; set; }

        public string GoalName { get; set; }
    }
}
