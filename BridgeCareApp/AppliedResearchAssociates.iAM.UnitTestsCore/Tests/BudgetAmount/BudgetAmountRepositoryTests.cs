using System;
using System.Collections.Generic;
using System.Linq;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using Xunit;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests.BudgetAmount
{
    public class BudgetAmountRepositoryTests
    {
        [Fact]
        public void UpsertOrDeleteBudgetAmounts_LibraryInDbWithBudgets_Adds()
        {
            var library = BudgetLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, "Old name");
            var libraryId = library.Id;
            var budgetId = Guid.NewGuid();
            var budgetDto = BudgetDtos.New(budgetId);
            var budgetDtoWithLibraryId = new BudgetDTOWithLibraryId
            {
                Budget = budgetDto,
                BudgetLibraryId = libraryId,
            };
            var budgetDtoWithLibraryIds = new List<BudgetDTOWithLibraryId> { budgetDtoWithLibraryId };
            TestHelper.UnitOfWork.BudgetRepo.AddBudgets(budgetDtoWithLibraryIds);
            var budgetAmountDto = BudgetAmountDtos.ForBudgetAndYear(budgetDto, 2025, 2718.28m);
            var budgetAmountList = new List<BudgetAmountDTO> { budgetAmountDto };
            var budgetAmountDictionary = new Dictionary<Guid, List<BudgetAmountDTO>> { { budgetId, budgetAmountList } };

            TestHelper.UnitOfWork.BudgetAmountRepo.UpsertOrDeleteBudgetAmounts(
                budgetAmountDictionary, libraryId);

            var budgetsAfter = TestHelper.UnitOfWork.BudgetRepo.GetLibraryBudgets(libraryId);
            var budgetAfter = budgetsAfter.Single();
            var amountAfter = budgetAfter.BudgetAmounts.Single();
            ObjectAssertions.Equivalent(budgetAmountDto, amountAfter);
        }

        [Fact]
        public void UpsertOrDeleteBudgetAmounts_LibraryInDbWithBudgetWithAmount_Updates()
        {
            var library = BudgetLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, "Old name");
            var libraryId = library.Id;
            var budgetId = Guid.NewGuid();
            var amountId = Guid.NewGuid();
            var budgetDto = BudgetDtos.WithSingleAmount(
                budgetId, "Budget", 2025, 655.36m, amountId);
            var budgetDtoWithLibraryId = new BudgetDTOWithLibraryId
            {
                Budget = budgetDto,
                BudgetLibraryId = libraryId,
            };
            var budgetDtoWithLibraryIds = new List<BudgetDTOWithLibraryId> { budgetDtoWithLibraryId };
            TestHelper.UnitOfWork.BudgetRepo.AddBudgets(budgetDtoWithLibraryIds);
            var updatedBudgetAmountDto = BudgetAmountDtos.ForBudgetAndYear(budgetDto, 2025, 2718.28m, amountId);
            var budgetAmountList = new List<BudgetAmountDTO> { updatedBudgetAmountDto };
            var budgetAmountDictionary = new Dictionary<Guid, List<BudgetAmountDTO>> { { budgetId, budgetAmountList } };

            TestHelper.UnitOfWork.BudgetAmountRepo.UpsertOrDeleteBudgetAmounts(
                budgetAmountDictionary, libraryId);

            var budgetsAfter = TestHelper.UnitOfWork.BudgetRepo.GetLibraryBudgets(libraryId);
            var budgetAfter = budgetsAfter.Single();
            var amountAfter = budgetAfter.BudgetAmounts.Single();
            ObjectAssertions.Equivalent(updatedBudgetAmountDto, amountAfter);
        }

        [Fact]
        public void UpsertOrDeleteBudgetAmounts_DictionaryIsEmpty_DeletesExistingAmounts()
        {
            var library = BudgetLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, "Old name");
            var libraryId = library.Id;
            var budgetId = Guid.NewGuid();
            var amountId = Guid.NewGuid();
            var budgetDto = BudgetDtos.WithSingleAmount(
                budgetId, "Budget", 2025, 655.36m, amountId);
            var budgetDtoWithLibraryId = new BudgetDTOWithLibraryId
            {
                Budget = budgetDto,
                BudgetLibraryId = libraryId,
            };
            var budgetDtoWithLibraryIds = new List<BudgetDTOWithLibraryId> { budgetDtoWithLibraryId };
            TestHelper.UnitOfWork.BudgetRepo.AddBudgets(budgetDtoWithLibraryIds);
            var budgetAmountList = new List<BudgetAmountDTO> { };
            var budgetAmountDictionary = new Dictionary<Guid, List<BudgetAmountDTO>> { { budgetId, budgetAmountList } };

            TestHelper.UnitOfWork.BudgetAmountRepo.UpsertOrDeleteBudgetAmounts(
                budgetAmountDictionary, libraryId);

            var budgetsAfter = TestHelper.UnitOfWork.BudgetRepo.GetLibraryBudgets(libraryId);
            var budgetAfter = budgetsAfter.Single();
            Assert.Empty(budgetAfter.BudgetAmounts);
        }
    }
}
