using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class TargetConditionGoalLibraryDTO : BaseLibraryDTO
    {
        public List<TargetConditionGoalDTO> TargetConditionGoals { get; set; }
    }
}
