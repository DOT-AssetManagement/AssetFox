using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;
using AssetFox.Core.UnitTestsCore.TestUtils;

namespace AssetFox.Core.UnitTestsCore.Tests.TargetConditionGoal
{
    public static class TargetConditionGoalTestSetup
    {
        public static TargetConditionGoalDTO ModelForLibraryGoalInDb(Guid targetConditionGoalLibraryId, string attributeName)
        {
            var targetConditionGoal = TargetConditionGoalDtos.Dto(attributeName);
            var targetConditionGoals = new List<TargetConditionGoalDTO> { targetConditionGoal };
            TestHelper.UnitOfWork.TargetConditionGoalRepo.UpsertOrDeleteTargetConditionGoals(targetConditionGoals, targetConditionGoalLibraryId);
            return targetConditionGoal;
        }
    }
}
