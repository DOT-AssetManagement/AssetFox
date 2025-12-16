using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;

namespace AssetFox.Core.UnitTestsCore.Tests.BudgetPriority
{
    public static class BudgetPriorityLibraryTestSetup
    {
        public static BudgetPriorityLibraryDTO CreateBudgetPriorityLibraryDto(string name)
        {
            var dto = BudgetPriorityDtos.New(null, 1, 2023);
            var budgetPriorityList = new List<BudgetPriorityDTO> { dto };

            return new BudgetPriorityLibraryDTO()
            {
                Id = Guid.NewGuid(),
                Name = name,
                BudgetPriorities = budgetPriorityList?.ToList(),
            };
        }
        public static BudgetPriorityLibraryDTO ModelForEntityInDb(IUnitOfWork unitOfWork, string budgetPriorityLibraryName = null)
        {
            var resolveBudgetPriorityLibraryName = budgetPriorityLibraryName ?? RandomStrings.WithPrefix("BudgetPriorityLibrary");
            var dto = CreateBudgetPriorityLibraryDto(resolveBudgetPriorityLibraryName);
            unitOfWork.BudgetPriorityRepo.UpsertBudgetPriorityLibrary(dto);
            var dtoAfter = unitOfWork.BudgetPriorityRepo.GetBudgetPriorityLibraries().FirstOrDefault(x => x.Id == dto.Id);
            return dtoAfter;
        }
    }
}
