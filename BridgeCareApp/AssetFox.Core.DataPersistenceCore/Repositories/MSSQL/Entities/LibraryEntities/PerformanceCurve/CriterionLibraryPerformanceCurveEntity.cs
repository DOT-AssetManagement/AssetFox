using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.PerformanceCurve
{
    public class CriterionLibraryPerformanceCurveEntity : BaseCriterionLibraryJoinEntity
    {
        public Guid PerformanceCurveId { get; set; }

        public virtual PerformanceCurveEntity PerformanceCurve { get; set; }
    }
}
