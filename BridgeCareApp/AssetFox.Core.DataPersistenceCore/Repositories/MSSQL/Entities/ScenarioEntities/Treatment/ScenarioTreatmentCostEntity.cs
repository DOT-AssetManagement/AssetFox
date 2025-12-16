using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment
{
    public class ScenarioTreatmentCostEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid ScenarioSelectableTreatmentId { get; set; }

        public virtual ScenarioSelectableTreatmentEntity ScenarioSelectableTreatment { get; set; }

        public virtual CriterionLibraryScenarioTreatmentCostEntity CriterionLibraryScenarioTreatmentCostJoin { get; set; }

        public virtual ScenarioTreatmentCostEquationEntity ScenarioTreatmentCostEquationJoin { get; set; }
    }
}
