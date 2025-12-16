using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.SimulationCloning
{
    public class DeficientConditionGoalDetailCloner
    {
        internal static IList<DeficientConditionGoalDetailDTO> CloneList(IList<DeficientConditionGoalDetailDTO> deficientConditionGoals)
        {
            var clone = new List<DeficientConditionGoalDetailDTO>();

            foreach (var deficientConditionGoal in deficientConditionGoals)
            {
                var childClone = Clone(deficientConditionGoal);
                clone.Add(childClone);
            }

            return clone;
        }

        internal static DeficientConditionGoalDetailDTO Clone(DeficientConditionGoalDetailDTO deficientConditionGoal)
        {
            return new DeficientConditionGoalDetailDTO
            {
                Id = Guid.NewGuid(),
                ActualDeficientPercentage = deficientConditionGoal.ActualDeficientPercentage,
                AllowedDeficientPercentage = deficientConditionGoal.AllowedDeficientPercentage,
                DeficientLimit = deficientConditionGoal.DeficientLimit,
                AttributeId = deficientConditionGoal.AttributeId,
                GoalIsMet=deficientConditionGoal.GoalIsMet,
                GoalName=deficientConditionGoal.GoalName                
            };
        }
    }
}
