using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class BudgetDetailDTO : BaseDTO
    {
        public decimal AvailableFunding { get; set; }

        public string BudgetName { get; set; }
    }
}
