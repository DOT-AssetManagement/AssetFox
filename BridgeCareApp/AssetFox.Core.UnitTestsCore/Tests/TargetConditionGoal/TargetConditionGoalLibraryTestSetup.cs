using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;

namespace AssetFox.Core.UnitTestsCore.Tests.TargetConditionGoal
{
    public class TargetConditionGoalLibraryTestSetup
    {
        private static CriterionLibraryDTO CreateCriterionLibraryObject(string criteriaExpression = "", bool singleUse = false)
        {
            return new CriterionLibraryDTO()
            {
                MergedCriteriaExpression = criteriaExpression,
                IsSingleUse = singleUse
            };
        }
        private static TargetConditionGoalDTO CreateTargetConditionGoalDto(string targetConditionGoalName)
        {
            //create criterion library
            var criterionLibraryObject = CreateCriterionLibraryObject("0=0", true);

            return new TargetConditionGoalDTO()
            {
                Id = Guid.NewGuid(),
                Name = targetConditionGoalName,
                Attribute = "testAttribute",
                Year = 2020,
                Target = 0,
                CriterionLibrary = criterionLibraryObject
            };
        }
        public static TargetConditionGoalLibraryDTO LibraryDtoWithSingleGoal(string libraryName, Guid id)
        {
            var targetConditionGoalList = new List<TargetConditionGoalDTO>();
            targetConditionGoalList.Add(CreateTargetConditionGoalDto("Target Condition Goal 1"));

            return new TargetConditionGoalLibraryDTO()
            {
                Id = id,
                Name = libraryName,
                TargetConditionGoals = targetConditionGoalList?.ToList(),
            };
        }

        public static TargetConditionGoalLibraryDTO ModelForEntityInDbWithSingleGoal(IUnitOfWork unitOfWork, string targetConditionGoalLibraryName = null, Guid? libraryId = null)
        {
            var resolveTargetConditionGoalLibraryName = targetConditionGoalLibraryName ?? RandomStrings.WithPrefix("TargetConditionGoalLibrary");
            var resolveId = libraryId ?? Guid.NewGuid();
            var dto = LibraryDtoWithSingleGoal(resolveTargetConditionGoalLibraryName, resolveId);
            unitOfWork.TargetConditionGoalRepo.UpsertTargetConditionGoalLibrary(dto);
            return dto;
        }
    }
}
