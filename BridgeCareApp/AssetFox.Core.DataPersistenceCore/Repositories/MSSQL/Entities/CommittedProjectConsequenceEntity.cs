using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class CommittedProjectConsequenceEntity : TreatmentConsequenceEntity
    {
        public Guid CommittedProjectId { get; set; }

        public float PerformanceFactor { get; set; }

        public virtual CommittedProjectEntity CommittedProject { get; set; }
    }
}
