using System;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IAnalysisMethodRepository
    {
        void GetSimulationAnalysisMethod(Simulation simulation, string userCriteria);

        AnalysisMethodDTO GetAnalysisMethod(Guid simulationId);

        bool GetSimulationAnalysisMethodSetting(Guid simulationId);

        void UpsertAnalysisMethod(Guid simulationId, AnalysisMethodDTO dto);
    }
}
