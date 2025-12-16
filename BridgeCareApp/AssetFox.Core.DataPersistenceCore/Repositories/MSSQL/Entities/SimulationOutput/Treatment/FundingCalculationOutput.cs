using System;
using System.Collections.Generic;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class FundingCalculationOutput
    {
        public Guid Id { get; set; }

        public ICollection<Allocation> AllocationMatrix { get; set; } = new HashSet<Allocation>();

        public Guid TreatmentConsiderationDetailId { get; set; }

        public int RunId { get; set; }

        public virtual TreatmentConsiderationDetailEntity TreatmentConsiderationDetail { get; set; }
    }

    public class Allocation
    {
        public Guid Id { get; set; }

        public int Year { get; set; }

        public string BudgetName { get; set; }

        public string TreatmentName { get; set; }

        public decimal AllocatedAmount { get; set; }

        public Guid FundingCalculationOutputId { get; set; }

        public virtual FundingCalculationOutput FundingCalculationOutput { get; set; }

        public int RunId { get; set; }
    }
}
