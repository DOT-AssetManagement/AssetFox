using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class AllocationDTO : BaseDTO
    {
        public int Year { get; set; }

        public string BudgetName { get; set; }

        public string TreatmentName { get; set; }

        public decimal AllocatedAmount { get; set; }        
    }
}
