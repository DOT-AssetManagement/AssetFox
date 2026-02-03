using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Treatment
{
    public class TreatmentSchedulingEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid TreatmentId { get; set; }

        public int OffsetToFutureYear { get; set; }

        public virtual SelectableTreatmentEntity SelectableTreatment { get; set; }
    }
}
