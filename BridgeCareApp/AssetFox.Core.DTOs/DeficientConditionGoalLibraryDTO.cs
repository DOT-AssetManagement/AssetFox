using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class DeficientConditionGoalLibraryDTO : BaseLibraryDTO
    {
        public List<DeficientConditionGoalDTO> DeficientConditionGoals { get; set; }
    }
}
