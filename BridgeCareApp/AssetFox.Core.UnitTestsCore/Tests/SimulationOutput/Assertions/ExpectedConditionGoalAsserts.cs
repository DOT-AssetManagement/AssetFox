using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Analysis;
using AssetFox.Core.Analysis.Engine;
using Xunit;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class DeficientConditionGoalDetailAsserts
    {
        public static void Same(DeficientConditionGoalDetail expected, DeficientConditionGoalDetail actual)
        {
            Assert.Equal(expected.DeficientLimit, actual.DeficientLimit);
            Assert.Equal(expected.ActualDeficientPercentage, actual.ActualDeficientPercentage);
            Assert.Equal(expected.AllowedDeficientPercentage, actual.AllowedDeficientPercentage);
            Assert.Equal(expected.GoalName, actual.GoalName);
            Assert.Equal(expected.AttributeName, actual.AttributeName);
        }
    }
}
