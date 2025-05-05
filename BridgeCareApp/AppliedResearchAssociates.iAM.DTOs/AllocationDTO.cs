using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class AllocationDTO : BaseDTO
    {
        public int Year { get; set; }

        public string BudgetName { get; set; }

        public string TreatmentName { get; set; }

        public decimal AllocatedAmount { get; set; }        
    }
}
