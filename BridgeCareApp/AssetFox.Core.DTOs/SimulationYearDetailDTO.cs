using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class SimulationYearDetailDTO : BaseDTO
    {
        public double ConditionOfNetwork { get; set; }

        public int Year { get; set; }

        public IList<BudgetDetailDTO> Budgets { get; set; } = new List<BudgetDetailDTO>();

        public IList<DeficientConditionGoalDetailDTO> DeficientConditionGoals { get; set; } = new List<DeficientConditionGoalDetailDTO>();

        public IList<AssetDetailDTO> Assets { get; set; } = new List<AssetDetailDTO>();

        public IList<TargetConditionGoalDetailDTO> TargetConditionGoals { get; set; } = new List<TargetConditionGoalDetailDTO>();
    }
}
