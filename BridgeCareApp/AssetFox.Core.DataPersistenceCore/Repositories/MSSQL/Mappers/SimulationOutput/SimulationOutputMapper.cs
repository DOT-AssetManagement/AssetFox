using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class SimulationOutputMapper
    {
        public static SimulationOutputEntity ToEntityWithoutAssetsOrYearDetails(
            this SimulationOutput domain,
            Guid simulationId,
            Dictionary<string, Guid> attributeIdLookup)
        {
            var id = Guid.NewGuid();
            var years = new List<SimulationYearDetailEntity>();
            var summaryEntities = new List<AssetSummaryDetailEntity>();
            var entity = new SimulationOutputEntity
            {
                Id = id,
                InitialConditionOfNetwork = domain.InitialConditionOfNetwork,
                SimulationId = simulationId,
                Years = years,
                InitialAssetSummaries = summaryEntities,
            };

            return entity;
        }

        public static SimulationOutput ToDomainWithoutAssets(SimulationOutputEntity entity,
            Dictionary<Guid, string> attributeNameLookup)
        {
            var simulationLastModifiedDate = entity.Simulation.LastModifiedDate;
            var outputLastModifiedDate = entity.LastModifiedDate;
            var lastModifiedDate = outputLastModifiedDate > simulationLastModifiedDate ? outputLastModifiedDate : simulationLastModifiedDate; 
            var domain = new SimulationOutput
            {
                InitialConditionOfNetwork = entity.InitialConditionOfNetwork,
                LastModifiedDate = lastModifiedDate,
            };
            var years = SimulationYearDetailMapper.ToDomainListWithoutAssets(entity.Years, attributeNameLookup);
            domain.Years.AddRange(years);

            return domain;
        }

        public static SimulationOutputDTO ToDtoWithoutYears(this SimulationOutputEntity entity)
        {
            var simulationOutput = new SimulationOutputDTO
            {
                Id = entity.Id,
                InitialConditionOfNetwork = entity.InitialConditionOfNetwork,
                InitialAssetSummaries = entity.InitialAssetSummaries.Select(_ => _.ToDto()).ToList(),
                RunId = entity.RunId,
            };

            return simulationOutput;
        }

        internal static SimulationOutputEntity ToEntity(this SimulationOutputDTO simulationOutputDto, Guid simulationId, int simulationRunId)
        {
            var simulationOutputId = simulationOutputDto.Id;

            return new SimulationOutputEntity
            {
                Id = simulationOutputId,
                RunId = simulationRunId,
                SimulationId = simulationId,
                InitialConditionOfNetwork = simulationOutputDto.InitialConditionOfNetwork,
                InitialAssetSummaries = simulationOutputDto.InitialAssetSummaries.Select(_ => _.ToEntity(simulationOutputId, simulationRunId)).ToList(),
                Years = simulationOutputDto.Years.Select(_ => _.ToEntity(simulationOutputId)).ToList()
            };
        }
    }
}
