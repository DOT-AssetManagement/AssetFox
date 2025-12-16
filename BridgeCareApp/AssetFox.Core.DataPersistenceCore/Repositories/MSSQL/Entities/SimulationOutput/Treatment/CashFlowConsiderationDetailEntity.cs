using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class CashFlowConsiderationDetailEntity
    {
        public Guid Id { get; set; }

        public Guid TreatmentConsiderationDetailId { get; set; }

        public virtual TreatmentConsiderationDetailEntity TreatmentConsiderationDetail { get; set; }

        public string CashFlowRuleName { get; set; }

        public int ReasonAgainstCashFlow { get; set; }
        public int RunId { get; set; }
    }
}
