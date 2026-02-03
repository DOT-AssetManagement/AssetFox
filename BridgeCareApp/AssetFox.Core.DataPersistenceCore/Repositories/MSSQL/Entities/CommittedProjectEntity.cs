using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Budget;
using AssetFox.Core.DTOs.Enums;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class CommittedProjectEntity : CommittedTreatmentEntity
    {
        public Guid SimulationId { get; set; }

        public Guid? ScenarioBudgetId { get; set; }

        public double Cost { get; set; }

        public int Year { get; set; }

        public string Category { get; set; }

        public TreatmentCategory treatmentCategory { get; set; }

        public virtual SimulationEntity Simulation { get; set; }

        public virtual ScenarioBudgetEntity ScenarioBudget { get; set; }

        public virtual CommittedProjectLocationEntity CommittedProjectLocation { get; set; }

        public string ProjectSource { get; set; }

        public string ProjectId { get; set; }
    }
}
