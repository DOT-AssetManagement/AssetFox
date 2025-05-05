using System;
using System.Collections.Generic;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class TreatmentConsiderationDetailEntity
    {
        public Guid Id { get; set; }

        public Guid AssetDetailId { get; set; }

        public virtual AssetDetailEntity AssetDetail { get; set; }

        public int? BudgetPriorityLevel { get; set; }

        public ICollection<CashFlowConsiderationDetailEntity> CashFlowConsiderations { get; set; } = new HashSet<CashFlowConsiderationDetailEntity>();

        public string TreatmentName { get; set; }
        public int RunId { get; set; }

        public virtual FundingCalculationInput FundingCalculationInput { get; set; }

        public virtual FundingCalculationOutput FundingCalculationOutput { get; set; }
    }
}
