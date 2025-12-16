using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class TreatmentRejectionDetailEntity
    {
        public Guid Id { get; set; }

        public Guid AssetDetailId { get; set; }

        public virtual AssetDetailEntity AssetDetail { get; set; }

        public double PotentialConditionChange { get; set; }

        public string TreatmentName { get; set; }

        public int TreatmentRejectionReason { get; set; }
        public int RunId { get; set; } 
    }
}
