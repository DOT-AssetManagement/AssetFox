using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Migrations;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.CalculatedAttribute
{
    public class ScenarioCalculatedAttributeEntity : BaseCalculatedAttributeEntity
    {
        public ScenarioCalculatedAttributeEntity() => Equations = new HashSet<ScenarioCalculatedAttributeEquationCriteriaPairEntity>();

        public Guid SimulationId { get; set; }

        public Guid LibraryId { get; set; }

        public bool IsModified { get; set; }

        public virtual SimulationEntity Simulation { get; set; }

        public ICollection<ScenarioCalculatedAttributeEquationCriteriaPairEntity> Equations { get; set; }
    }
}
