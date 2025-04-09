using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class FundingCalculationInputDTO : BaseDTO
    {
        public Guid TreatmentConsiderationDetailId { get; set; }

        public IList<BudgetToSpendDTO> CurrentBudgetsToSpend { get; set; } = new List<BudgetToSpendDTO>();        
    }
}
