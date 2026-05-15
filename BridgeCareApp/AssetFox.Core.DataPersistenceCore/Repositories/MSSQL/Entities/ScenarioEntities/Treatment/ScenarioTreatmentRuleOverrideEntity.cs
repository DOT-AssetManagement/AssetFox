using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment
{
    public class ScenarioTreatmentRuleOverrideEntity : BaseEntity
    {
        public ScenarioTreatmentRuleOverrideEntity()
        {
            Criteria = new HashSet<ScenarioTreatmentRuleOverrideCriterionEntity>();
        }

        public Guid Id { get; set; }

        public Guid SimulationId { get; set; }

        public Guid? SourceTreatmentRuleOverrideId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsEnabled { get; set; } = true;

        public virtual SimulationEntity Simulation { get; set; }

        public virtual ICollection<ScenarioTreatmentRuleOverrideCriterionEntity> Criteria { get; set; }
    }
}
