using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class BudgetPriorityLibraryDTO : BaseLibraryDTO
    {
        public List<BudgetPriorityDTO> BudgetPriorities { get; set; }
    }
}
