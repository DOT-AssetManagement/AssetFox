using System.Collections.Generic;

namespace AssetFox.Core.DTOs.Abstract
{
    public class ScenarioBudgetImportResultDTO : WarningServiceResultDTO
    {
        public List<BudgetDTO> Budgets { get; set; }
    }
}
