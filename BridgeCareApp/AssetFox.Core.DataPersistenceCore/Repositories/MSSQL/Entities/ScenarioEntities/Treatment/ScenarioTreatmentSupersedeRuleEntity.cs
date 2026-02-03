using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment
{
    public class ScenarioTreatmentSupersedeRuleEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid TreatmentId { get; set; }

        public virtual ScenarioSelectableTreatmentEntity ScenarioSelectableTreatment { get; set; }

        public Guid PreventTreatmentId { get; set; }        

        public virtual CriterionLibraryScenarioTreatmentSupersedeRuleEntity CriterionLibraryScenarioTreatmentSupersedeRuleJoin { get; set; }
    }  
}
