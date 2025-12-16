using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.PerformanceCurve
{
    public class CriterionLibraryScenarioPerformanceCurveEntity : BaseCriterionLibraryJoinEntity
    {
        public Guid ScenarioPerformanceCurveId { get; set; }

        public virtual ScenarioPerformanceCurveEntity ScenarioPerformanceCurve { get; set; }
    }
}
