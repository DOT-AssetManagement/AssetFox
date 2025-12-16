using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class BudgetDetailDTO : BaseDTO
    {
        public decimal AvailableFunding { get; set; }

        public string BudgetName { get; set; }
    }
}
