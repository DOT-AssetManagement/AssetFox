using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Enums;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers;

public static class SimulationOutputJsonMapper
{
    /// <summary>
    /// Without simulation output mapping
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="simulationId"></param>
    /// <returns>SimulationOutputJsonEntity</returns>
    public static SimulationOutputJsonEntity ToEntity(this SimulationOutputJsonDTO dto, Guid simulationId)
    {
        var entity = new SimulationOutputJsonEntity()
        {
            Id = dto.Id,
            SimulationId = simulationId,
            Output = dto.Output,
            OutputType = (SimulationOutputEnum)dto.OutputType,
        };
        
        return entity;
    }
}
