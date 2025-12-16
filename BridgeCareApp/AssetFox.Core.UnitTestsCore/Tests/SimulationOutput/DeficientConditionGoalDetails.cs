using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Analysis.Engine;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class DeficientConditionGoalDetails
    {
        public static DeficientConditionGoalDetail Detail(string attributeName)
        {
            var detail = new DeficientConditionGoalDetail
            {
                ActualDeficientPercentage = 10,
                AllowedDeficientPercentage = 20,
                AttributeName = attributeName,
                DeficientLimit = 30,
                GoalIsMet = false,
                GoalName = "Goal",
            };
            return detail;
        }
    }
}
