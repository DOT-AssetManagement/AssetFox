using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface ISimulationLogRepository
    {
        void ClearLog(Guid simulationId);
        void CreateLogs(IList<SimulationLogDTO> dtos);
        Task<List<SimulationLogDTO>> GetLog(Guid simulationId);
    }

    public static class ISimulationLogRepositoryExtensions
    {
        public static void CreateLog(this ISimulationLogRepository repository,
            List<SimulationLogDTO> dtos)
        {
            var list = dtos;
            repository.CreateLogs(list);
        }
    }
}
