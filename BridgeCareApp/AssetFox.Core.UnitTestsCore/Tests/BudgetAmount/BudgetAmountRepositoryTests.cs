using System;
using System.Collections.Generic;
using System.Linq;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Repositories;
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
            var budgetDto = BudgetDtos.New(
                budgetId, "Budget");
            var budgetDtoWithLibraryId = new BudgetDTOWithLibraryId
            {
                Budget = budgetDto,
                BudgetLibraryId = libraryId,
            };
            var budgetDtoWithLibraryIds = new List<BudgetDTOWithLibraryId> { budgetDtoWithLibraryId };
            TestHelper.UnitOfWork.BudgetRepo.AddBudgets(budgetDtoWithLibraryIds);
            BudgetAmountTestSetup.LibraryAmountInDb(budgetId, amountId, budgetDto);
            var amountsBefore = TestHelper.UnitOfWork.BudgetAmountRepo.GetLibraryBudgetAmounts(libraryId);
            Assert.Single(amountsBefore);
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
            var budgetDto = BudgetDtos.New(
                budgetId, "Budget");
            var budgetDtoWithLibraryId = new BudgetDTOWithLibraryId
            {
                Budget = budgetDto,
                BudgetLibraryId = libraryId,
            };
            var budgetDtoWithLibraryIds = new List<BudgetDTOWithLibraryId> { budgetDtoWithLibraryId };
            TestHelper.UnitOfWork.BudgetRepo.AddBudgets(budgetDtoWithLibraryIds);
            BudgetAmountTestSetup.LibraryAmountInDb(budgetId, amountId, budgetDto);
            var amountsBefore = TestHelper.UnitOfWork.BudgetAmountRepo.GetLibraryBudgetAmounts(libraryId);
            Assert.Single(amountsBefore);
            var budgetAmountList = new List<BudgetAmountDTO> { };
            var budgetAmountDictionary = new Dictionary<Guid, List<BudgetAmountDTO>> { { budgetId, budgetAmountList } };

            TestHelper.UnitOfWork.BudgetAmountRepo.UpsertOrDeleteBudgetAmounts(
                budgetAmountDictionary, libraryId);

            var budgetsAfter = TestHelper.UnitOfWork.BudgetRepo.GetLibraryBudgets(libraryId);
            var budgetAfter = budgetsAfter.Single();
            Assert.Empty(budgetAfter.BudgetAmounts);
        }

        [Fact]
        public void UpsertOrDeleteScenarioBudgetAmounts_LibraryInDbWithBudgets_Adds()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var budgetId = Guid.NewGuid();
            var budgetDto = BudgetDtos.New(budgetId);
            var budgetDtos = new List<BudgetDTO> { budgetDto };
            TestHelper.UnitOfWork.BudgetRepo.UpsertOrDeleteScenarioBudgets(budgetDtos, simulation.Id);
            var budgetAmountDto = BudgetAmountDtos.ForBudgetAndYear(budgetDto, 2025, 2718.28m);
            var budgetAmountList = new List<BudgetAmountDTO> { budgetAmountDto };
            var budgetAmountDictionary = new Dictionary<Guid, List<BudgetAmountDTO>> { { budgetId, budgetAmountList } };

            TestHelper.UnitOfWork.BudgetAmountRepo.UpsertOrDeleteScenarioBudgetAmounts(
                budgetAmountDictionary, simulation.Id);

            var budgetsAfter = TestHelper.UnitOfWork.BudgetRepo.GetScenarioBudgets(simulation.Id);
            var budgetAfter = budgetsAfter.Single();
            var amountAfter = budgetAfter.BudgetAmounts.Single();
            ObjectAssertions.Equivalent(budgetAmountDto, amountAfter);
        }

        [Fact]
        public void UpsertOrDeleteScenarioBudgetAmounts_AmountAlreadyExists_Updates()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var budgetId = Guid.NewGuid();
            var amountId = Guid.NewGuid();
            var budgetDto = BudgetDtos.New(budgetId, "Budget");
            var budgetDtos = new List<BudgetDTO> { budgetDto };
            TestHelper.UnitOfWork.BudgetRepo.UpsertOrDeleteScenarioBudgets(budgetDtos, simulation.Id);
            var oldBudgetAmountDto = BudgetAmountTestSetup.SetupSingleScenarioAmountForBudget(TestHelper.UnitOfWork,
                 simulation.Id, "Budget", budgetId, amountId);
            var budgetAmountsBefore = TestHelper.UnitOfWork.BudgetAmountRepo.GetScenarioBudgetAmounts(simulation.Id);
            Assert.Single(budgetAmountsBefore);
            var budgetAmountDto = BudgetAmountDtos.ForBudgetAndYear(budgetDto, 2025, 11111m, amountId);
            var budgetAmountList = new List<BudgetAmountDTO> { budgetAmountDto };
            var budgetAmountDictionary = new Dictionary<Guid, List<BudgetAmountDTO>> { { budgetId, budgetAmountList } };

            TestHelper.UnitOfWork.BudgetAmountRepo.UpsertOrDeleteScenarioBudgetAmounts(
                budgetAmountDictionary, simulation.Id);

            var budgetsAfter = TestHelper.UnitOfWork.BudgetRepo.GetScenarioBudgets(simulation.Id);
            var budgetAfter = budgetsAfter.Single();
            var amountAfter = budgetAfter.BudgetAmounts.Single();
            ObjectAssertions.Equivalent(budgetAmountDto, amountAfter);
        }


        [Fact]
        public void UpsertOrDeleteScenarioBudgetAmounts_AmountNotInDictionary_Deletes()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var budgetId = Guid.NewGuid();
            var amountId = Guid.NewGuid();
            var budgetDto = BudgetDtos.New(budgetId, "Budget");
            var budgetDtos = new List<BudgetDTO> { budgetDto };
            TestHelper.UnitOfWork.BudgetRepo.UpsertOrDeleteScenarioBudgets(budgetDtos, simulation.Id);
            var oldBudgetAmountDto = BudgetAmountTestSetup.SetupSingleScenarioAmountForBudget(TestHelper.UnitOfWork,
                simulation.Id, "Budget", budgetId, amountId);
            var budgetAmountsBefore = TestHelper.UnitOfWork.BudgetAmountRepo.GetScenarioBudgetAmounts(simulation.Id);
            Assert.Single(budgetAmountsBefore);
            var budgetAmountList = new List<BudgetAmountDTO> { };
            var budgetAmountDictionary = new Dictionary<Guid, List<BudgetAmountDTO>> { { budgetId, budgetAmountList } };

            TestHelper.UnitOfWork.BudgetAmountRepo.UpsertOrDeleteScenarioBudgetAmounts(
                budgetAmountDictionary, simulation.Id);

            var budgetsAfter = TestHelper.UnitOfWork.BudgetRepo.GetScenarioBudgets(simulation.Id);
            var budgetAfter = budgetsAfter.Single();
            Assert.Empty(budgetAfter.BudgetAmounts);
        }

        [Fact]
        public void GetLibraryBudgetAmounts_LibraryInDbWithAmount_Gets()
        {
            var library = BudgetLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, "Old name");
            var libraryId = library.Id;
            var budgetId = Guid.NewGuid();
            var amountId = Guid.NewGuid();
            var budgetDto = BudgetDtos.New(
                budgetId, "Budget");
            var budgetDtoWithLibraryId = new BudgetDTOWithLibraryId
            {
                Budget = budgetDto,
                BudgetLibraryId = libraryId,
            };
            var budgetDtoWithLibraryIds = new List<BudgetDTOWithLibraryId> { budgetDtoWithLibraryId };
            TestHelper.UnitOfWork.BudgetRepo.AddBudgets(budgetDtoWithLibraryIds);
            var amountDto = BudgetAmountTestSetup.LibraryAmountInDb(budgetId, amountId, budgetDto);

            var amountsAfter = TestHelper.UnitOfWork.BudgetAmountRepo.GetLibraryBudgetAmounts(libraryId);

            var amountAfter = amountsAfter.Single();
            ObjectAssertions.EquivalentExcluding(amountDto, amountAfter, a => a.Id);
        }

        [Fact]
        public void GetScenarioBudgetAmounts_AmountInDb_Gets()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var budgetId = Guid.NewGuid();
            var amountId = Guid.NewGuid();
            var budgetDto = BudgetDtos.New(budgetId, "Budget");
            var budgetDtos = new List<BudgetDTO> { budgetDto };
            TestHelper.UnitOfWork.BudgetRepo.UpsertOrDeleteScenarioBudgets(budgetDtos, simulation.Id);
            var oldBudgetAmountDto = BudgetAmountTestSetup.SetupSingleScenarioAmountForBudget(TestHelper.UnitOfWork,
                 simulation.Id, "Budget", budgetId, amountId);

            var amountsInDb = TestHelper.UnitOfWork.BudgetAmountRepo.GetScenarioBudgetAmounts(simulation.Id);
            var amountInDb = amountsInDb.Single();
            ObjectAssertions.EquivalentExcluding(oldBudgetAmountDto, amountInDb, a => a.Id);
        }
    }
}
