using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class BudgetToSpendDTO : BaseDTO
    {
        public string Name { get; set; }

        public decimal Amount { get; set; }

        public int Year { get; set; }        
    }
}
