using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class SimulationReportDetailEntity : BaseEntity
    {
        public Guid SimulationId { get; set; }

        public string Status { get; set; }

        public string ReportType { get; set; }
        public virtual SimulationEntity Simulation { get; set; }
    }
}
