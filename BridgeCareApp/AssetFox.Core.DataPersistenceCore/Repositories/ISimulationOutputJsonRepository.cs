using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories;

public interface ISimulationOutputJsonRepository
{
   List<SimulationOutputJsonDTO> GetSimulationOutputViaJson(Guid simulationId);
}
