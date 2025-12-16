using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class SimulationReportDetailMapper
    {
        public static SimulationReportDetailEntity ToEntity(this SimulationReportDetailDTO dto) =>
            new SimulationReportDetailEntity { SimulationId = dto.SimulationId, Status = dto.Status, ReportType = dto.ReportType };
    }
}
