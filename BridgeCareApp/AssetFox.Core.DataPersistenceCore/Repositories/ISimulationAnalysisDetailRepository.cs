using System;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface ISimulationAnalysisDetailRepository
    {
        void UpsertSimulationAnalysisDetail(SimulationAnalysisDetailDTO dto);

        SimulationAnalysisDetailDTO GetSimulationAnalysisDetail(Guid simulationId);
    }
}
