using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.PerformanceCurve
{
    public class ScenarioPerformanceCurveEntity : BasePerformanceCurveEntity
    {
        public Guid SimulationId { get; set; }

        public virtual SimulationEntity Simulation { get; set; }

        public Guid LibraryId { get; set; }

        public bool IsModified { get; set; }

        public virtual CriterionLibraryScenarioPerformanceCurveEntity CriterionLibraryScenarioPerformanceCurveJoin { get; set; }

        public virtual ScenarioPerformanceCurveEquationEntity ScenarioPerformanceCurveEquationJoin { get; set; }
    }
}
