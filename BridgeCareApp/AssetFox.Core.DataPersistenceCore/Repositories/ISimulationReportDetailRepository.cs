using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface ISimulationReportDetailRepository
    {
        void UpsertSimulationReportDetail(SimulationReportDetailDTO dto);
    }
}
