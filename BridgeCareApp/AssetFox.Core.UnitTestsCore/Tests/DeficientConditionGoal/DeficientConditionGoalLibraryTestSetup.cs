using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;

namespace AssetFox.Core.UnitTestsCore.Tests.DeficientConditionGoal
{
    public static class DeficientConditionGoalLibraryTestSetup
    {
        private static DeficientConditionGoalDTO CreateDeficientConditionGoalDto(string deficientConditionGoalName)
        {
            //create budget amounts
            //var budgetAmountList = new List<BudgetAmountDTO>();
            //budgetAmountList.Add(CreateBudgetAmountObject(budgetName, 2010, 2010000));

            //create criterion library
            //var criterionLibraryObject = CreateCriterionLibraryObject("0=0", true);

            return new DeficientConditionGoalDTO()
            {
                Id = Guid.NewGuid(),

                Name = deficientConditionGoalName,
                Attribute = "",
                AllowedDeficientPercentage = 100,
                DeficientLimit = 0,
                //CriterionLibrary = criterionLibraryObject
            };
        }
        public static DeficientConditionGoalLibraryDTO CreateDeficientConditionGoalLibraryDto(string name)
        {
            //setup
            var deficientConditionGoalList = new List<DeficientConditionGoalDTO>();
            deficientConditionGoalList.Add(CreateDeficientConditionGoalDto("Deficient Condition Goal 1"));

            //create budget library
            return new DeficientConditionGoalLibraryDTO()
            {
                Id = Guid.NewGuid(),
                Name = name,
            
                //Budgets = budgetList?.ToList(),
            };
        }
        public static DeficientConditionGoalLibraryDTO ModelForEntityInDb(IUnitOfWork unitOfWork, string deficientConditionGoalLibraryName = null)
        {
            var resolveDeficientConditionGoalLibraryName = deficientConditionGoalLibraryName ?? RandomStrings.WithPrefix("DeficientConditionGoalLibrary");
            var dto = CreateDeficientConditionGoalLibraryDto(resolveDeficientConditionGoalLibraryName);
            unitOfWork.DeficientConditionGoalRepo.UpsertDeficientConditionGoalLibrary(dto);
            var dtoAfter = unitOfWork.DeficientConditionGoalRepo.GetDeficientConditionGoalLibrariesNoChildren().FirstOrDefault(x => x.Id == dto.Id);
            return dtoAfter;
        }
    }
}
