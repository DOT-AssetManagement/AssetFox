using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class BudgetTestSetup
    {
        public static BudgetDTO AddBudgetToLibrary(
            UnitOfDataPersistenceWork unitOfWork,
            Guid libraryId,
            Guid? budgetId = null,
            Guid? criterionLibraryId = null)
        {
            CriterionLibraryDTO criterionLibraryDto = null;
            if (criterionLibraryId.HasValue)
            {
                var criterionLibraryName = RandomStrings.WithPrefix("CriterionLibrary");
                criterionLibraryDto = new CriterionLibraryDTO
                {
                    Id = criterionLibraryId.Value,
                    Name = criterionLibraryName,
                    MergedCriteriaExpression = "MergedCriteriaExpression",
                };
            }
            var resolvedBudgetId = budgetId ?? Guid.NewGuid();
            var budgetName = RandomStrings.WithPrefix("Budget");
            var amount = new BudgetAmountDTO
            {
                BudgetName = budgetName,
                Id = Guid.NewGuid(),
                Year = 2022,
                Value = 123456.78m,
            };
            var amounts = new List<BudgetAmountDTO> { amount };
            var budget = new BudgetDTO
            {
                Id = resolvedBudgetId,
                Name = budgetName,
                BudgetAmounts = amounts,
                CriterionLibrary = criterionLibraryDto,
            };
            var budgets = new List<BudgetDTO> { budget };
            unitOfWork.BudgetRepo.UpsertOrDeleteBudgets(budgets, libraryId);
            return budget;
        }
    }
}
