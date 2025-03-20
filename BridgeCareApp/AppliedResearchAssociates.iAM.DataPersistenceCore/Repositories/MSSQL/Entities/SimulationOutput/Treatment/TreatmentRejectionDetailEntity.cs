using System;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class TreatmentRejectionDetailEntity
    {
        public Guid Id { get; set; }

        public Guid AssetDetailId { get; set; }

        public virtual AssetDetailEntity AssetDetail { get; set; }

        public double PotentialConditionChange { get; set; }

        public string TreatmentName { get; set; }

        public int TreatmentRejectionReason { get; set; }
    }
}
