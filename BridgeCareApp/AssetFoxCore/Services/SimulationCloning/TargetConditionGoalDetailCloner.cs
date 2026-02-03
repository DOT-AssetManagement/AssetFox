using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.SimulationCloning
{
    public class TargetConditionGoalDetailCloner
    {
        internal static IList<TargetConditionGoalDetailDTO> CloneList(IList<TargetConditionGoalDetailDTO> targetConditionGoals)
        {
            var clone = new List<TargetConditionGoalDetailDTO>();

            foreach (var targetConditionGoal in targetConditionGoals)
            {
                var childClone = Clone(targetConditionGoal);
                clone.Add(childClone);
            }

            return clone;
        }

        internal static TargetConditionGoalDetailDTO Clone(TargetConditionGoalDetailDTO targetConditionGoal)
        {
            return new TargetConditionGoalDetailDTO
            {
                Id = Guid.NewGuid(),
                ActualValue = targetConditionGoal.ActualValue,
                TargetValue = targetConditionGoal.TargetValue,
                AttributeId = targetConditionGoal.AttributeId,
                GoalIsMet = targetConditionGoal.GoalIsMet,
                GoalName = targetConditionGoal.GoalName,
            };
        }
    }
}
