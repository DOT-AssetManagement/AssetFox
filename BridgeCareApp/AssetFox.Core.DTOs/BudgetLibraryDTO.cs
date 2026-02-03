using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class BudgetLibraryDTO : BaseLibraryDTO
    {
        public List<BudgetDTO> Budgets { get; set; }
    }
}
