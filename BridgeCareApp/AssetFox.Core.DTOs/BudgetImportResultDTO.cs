using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class BudgetImportResultDTO : WarningServiceResultDTO
    {
        public BudgetLibraryDTO BudgetLibrary { get; set; }
    }
}
