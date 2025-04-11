using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs;

namespace BridgeCareCore.Services.SimulationCloning
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
                ActualValue = targetConditionGoal.ActualValue,
                TargetValue = targetConditionGoal.TargetValue
            };
        }
    }
}
