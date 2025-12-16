using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class TreatmentSchedulingCollisionDetailEntity
    {
        public Guid Id { get; set; }

        public Guid AssetDetailId { get; set; }

        public int RunId { get; set; }

        public virtual AssetDetailEntity AssetDetail { get; set; }

        public string NameOfUnscheduledTreatment { get; set; }
    }
}
