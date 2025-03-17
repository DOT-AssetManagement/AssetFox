using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests
{
    internal class BudgetAmountTestSetup
    {
        public static BudgetAmountDTO SetupSingleScenarioAmountForBudget(UnitOfDataPersistenceWork unitOfWork, Guid simulationId, string budgetName, Guid budgetId, Guid budgetAmountId)
        {
            var budgetAmount = BudgetAmountDtos.ForBudgetNameAndYear(budgetName, 2023, 1234, budgetAmountId);
            var budgetAmountsPerBudgetId = new Dictionary<Guid, List<BudgetAmountDTO>>
            {
                {budgetId, new List<BudgetAmountDTO>{budgetAmount } }
            };
            unitOfWork.BudgetAmountRepo.UpsertOrDeleteScenarioBudgetAmounts(budgetAmountsPerBudgetId, simulationId);
            return budgetAmount;
        }

        public static BudgetAmountDTO LibraryAmountInDb(Guid budgetId, Guid amountId, BudgetDTO budgetDto)
        {
            var amountDto = BudgetAmountDtos.ForBudgetAndYear(budgetDto, 2025, 655.36m, amountId);
            var budgetAmountDtoWithBudgetId = new BudgetAmountDTOWithBudgetId
            {
                BudgetAmount = amountDto,
                BudgetId = budgetId,
            };
            var budgetDtosWithBudgetIds = new List<BudgetAmountDTOWithBudgetId> { budgetAmountDtoWithBudgetId };
            TestHelper.UnitOfWork.BudgetRepo.AddLibraryBudgetAmounts(budgetDtosWithBudgetIds);
            return amountDto;
        }
    }
}
