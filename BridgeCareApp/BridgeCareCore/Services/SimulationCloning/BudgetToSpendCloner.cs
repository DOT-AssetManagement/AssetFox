using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs;

namespace BridgeCareCore.Services.SimulationCloning
{
    public class BudgetToSpendCloner
    {
        internal static IList<BudgetToSpendDTO> CloneList(IList<BudgetToSpendDTO> currentBudgetsToSpend)
        {
            var cloneList = new List<BudgetToSpendDTO>();

            foreach (var currentBudgetsToSpen in currentBudgetsToSpend)
            {
                cloneList.Add(Clone(currentBudgetsToSpen));
            }

            return cloneList;
        }

        private static BudgetToSpendDTO Clone(BudgetToSpendDTO currentBudgetsToSpend)
        {
            return new BudgetToSpendDTO
            {
                Id = Guid.NewGuid(),
                Amount = currentBudgetsToSpend.Amount,
                Name = currentBudgetsToSpend.Name,
                Year = currentBudgetsToSpend.Year
            };
        }
    }
}
