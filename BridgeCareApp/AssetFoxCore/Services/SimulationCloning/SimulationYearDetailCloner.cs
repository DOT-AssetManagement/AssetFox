using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.SimulationCloning
{
    public class SimulationYearDetailCloner
    {
        internal static IList<SimulationYearDetailDTO> CloneList(IList<SimulationYearDetailDTO> years)
        {
            var cloneList = new List<SimulationYearDetailDTO>();

            foreach (var year in years)
            {
                cloneList.Add(Clone(year));
            }

            return cloneList;
        }

        internal static SimulationYearDetailDTO Clone(SimulationYearDetailDTO year)
        {
            var cloneAssets = AssetDetailCloner.CloneList(year.Assets);            
            var cloneBudgets = BudgetDetailCloner.CloneList(year.Budgets);
            var cloneDeficientConditionGoals = DeficientConditionGoalDetailCloner.CloneList(year.DeficientConditionGoals);
            var cloneTargetConditionGoals = TargetConditionGoalDetailCloner.CloneList(year.TargetConditionGoals);

            return new SimulationYearDetailDTO
            {
                Id = Guid.NewGuid(),
                ConditionOfNetwork = year.ConditionOfNetwork,
                Year = year.Year,
                Assets = cloneAssets,
                Budgets = cloneBudgets,
                DeficientConditionGoals = cloneDeficientConditionGoals,
                TargetConditionGoals = cloneTargetConditionGoals
            };
        }
    }
}
