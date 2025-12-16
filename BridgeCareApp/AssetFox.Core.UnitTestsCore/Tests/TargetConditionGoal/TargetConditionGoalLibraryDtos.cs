using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.TargetConditionGoal;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class TargetConditionGoalLibraryDtos
    {
        public static TargetConditionGoalLibraryDTO
            Dto(
            Guid? id = null,
            string name = null
            )
        {
            var resolvedId = id ?? Guid.NewGuid();
            var resolvedName = name ?? RandomStrings.Length11();
            var returnValue = new TargetConditionGoalLibraryDTO
            {
                Id = resolvedId,
                Name = resolvedName,
            };
            return returnValue;
        }
    }
}
