using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services
{
    public interface ICompleteSimulationCloningService
    {
        SimulationCloningResultDTO Clone(CloneSimulationDTO dto);
        CompleteSimulationDTO GetSimulation(string simulationId);
     
        bool CheckCompatibleNetworkAttributes(CloneSimulationDTO dto);
    }
}

