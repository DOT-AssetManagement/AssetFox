using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment
{
    public class ScenarioTreatmentRuleOverrideSettingEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid SimulationId { get; set; }

        public string OverrideName { get; set; }

        public bool IsEnabled { get; set; }

        public virtual SimulationEntity Simulation { get; set; }
    }
}
