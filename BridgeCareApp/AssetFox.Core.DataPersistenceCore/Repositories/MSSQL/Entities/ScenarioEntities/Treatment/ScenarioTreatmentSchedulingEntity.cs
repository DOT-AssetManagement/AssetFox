using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment
{
    public class ScenarioTreatmentSchedulingEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid TreatmentId { get; set; }

        public int OffsetToFutureYear { get; set; }

        public virtual ScenarioSelectableTreatmentEntity ScenarioSelectableTreatment { get; set; }
    }
}
