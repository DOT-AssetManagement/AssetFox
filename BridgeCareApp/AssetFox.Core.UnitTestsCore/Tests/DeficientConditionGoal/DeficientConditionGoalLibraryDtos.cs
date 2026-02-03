using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests.DeficientConditionGoal
{
    public static class DeficientConditionGoalLibraryDtos
    {
        public static DeficientConditionGoalLibraryDTO Empty(Guid? id = null)
        {
            var resolveId = id ?? Guid.NewGuid();
            var dto = new DeficientConditionGoalLibraryDTO
            {
                Id = resolveId,
                Name = "Deficient condition goal library",
            };
            return dto;
        }
    }
}
