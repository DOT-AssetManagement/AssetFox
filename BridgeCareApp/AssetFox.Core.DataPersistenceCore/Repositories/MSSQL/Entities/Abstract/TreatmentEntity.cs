using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract
{
    public abstract class TreatmentEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int ShadowForAnyTreatment { get; set; }

        public int ShadowForSameTreatment { get; set; }
    }
}
