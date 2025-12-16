using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class BudgetToSpendDTO : BaseDTO
    {
        public string Name { get; set; }

        public decimal Amount { get; set; }

        public int Year { get; set; }        
    }
}
