using System;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Enums;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class SimulationOutputJsonEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid SimulationId { get; set; }
        public Guid? SimulationOutputId { get; set; }
        public string Output { get; set; }

        public virtual SimulationEntity Simulation { get; set; }
        public virtual SimulationOutputEntity SimulationOutput {get;set;}

        public SimulationOutputEnum OutputType { get; set; }
    }
}
