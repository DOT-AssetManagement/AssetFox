using System;
using System.Collections.Generic;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class FundingCalculationInput
    {
        public Guid Id { get; set; }

        public ICollection<BudgetToSpend> CurrentBudgetsToSpend { get; set; } = new HashSet<BudgetToSpend>();

        public Guid TreatmentConsiderationDetailId { get; set; }

        public int RunId { get; set; }

        public virtual TreatmentConsiderationDetailEntity TreatmentConsiderationDetail { get; set; }
    }

    public class BudgetToSpend
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Amount { get; set; }

        public int Year { get; set; }

        public Guid FundingCalculationInputId { get; set; }

        public virtual FundingCalculationInput FundingCalculationInput { get; set; }

        public int RunId { get; set; }
    }
}
