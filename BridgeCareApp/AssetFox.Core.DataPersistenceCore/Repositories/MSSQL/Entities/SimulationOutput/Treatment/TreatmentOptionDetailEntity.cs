using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class TreatmentOptionDetailEntity
    {
        public Guid Id { get; set; }

        public Guid AssetDetailId { get; set; }

        public virtual AssetDetailEntity AssetDetail { get; set; }

        public double Benefit { get; set; }

        public double ConditionChange { get; set; }

        public double Cost { get; set; }

        public double? RemainingLife { get; set; }

        public string TreatmentName { get; set; }

        public int RunId { get; set; }
    }
}
