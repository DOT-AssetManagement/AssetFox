using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.SimulationCloning
{
    public class BudgetDetailCloner
    {
        internal static IList<BudgetDetailDTO> CloneList(IList<BudgetDetailDTO> budgets)
        {
            var clone = new List<BudgetDetailDTO>();

            foreach (var budget in budgets)
            {
                var childClone = Clone(budget);
                clone.Add(childClone);
            }

            return clone;
        }

        internal static BudgetDetailDTO Clone(BudgetDetailDTO budget)
        {
            return new BudgetDetailDTO
            {
                Id = Guid.NewGuid(),
                AvailableFunding = budget.AvailableFunding,
                BudgetName = budget.BudgetName
            };
        }
    }
}
