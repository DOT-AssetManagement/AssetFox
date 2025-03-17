using System;
using AppliedResearchAssociates.iAM.DTOs;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests
{
    public static class BudgetAmountDtos
    {
        public static BudgetAmountDTO ForBudgetAndYear(BudgetDTO budget, int year, decimal value = 1234, Guid? id = null)
        {
            var amount = ForBudgetNameAndYear(budget.Name, year, value, id);
            return amount;
        }

        public static BudgetAmountDTO ForBudgetNameAndYear(string budgetName, int year, decimal value = 1234, Guid? id = null)
        {
            var resolveId = id ?? Guid.NewGuid();
            var amount = new BudgetAmountDTO
            {
                Id = resolveId,
                BudgetName = budgetName,
                Year = year,
                Value = value
            };
            return amount;
        }
    }
}
