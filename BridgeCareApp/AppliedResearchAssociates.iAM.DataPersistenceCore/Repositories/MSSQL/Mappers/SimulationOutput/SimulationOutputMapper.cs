using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;
using AppliedResearchAssociates.iAM.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
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

        public static SimulationOutputDTO ToDto(this SimulationOutputEntity entity)
        {
            var simulationOutput = new SimulationOutputDTO
            {
                Id = entity.Id,
                InitialConditionOfNetwork = entity.InitialConditionOfNetwork,
                InitialAssetSummaries = entity.InitialAssetSummaries.Select(_ => _.ToDto()).ToList(),
                Years = entity.Years.Select(_ => _.ToDto()).ToList()
            };

            return simulationOutput;
        }

        internal static SimulationOutputEntity ToEntity(this SimulationOutputDTO simulationOutput, Guid simulationId)
        {
            // Assign relationals ids correctly
            // do not send in id int for AssetSummaryDetailValueEntityIntIdDTO and AssetDetailValueEntityIntIdDTO - it should be added as pk unique val
            return new SimulationOutputEntity
            {
                Id = simulationOutput.Id,
                SimulationId = simulationId,
                InitialConditionOfNetwork = simulationOutput.InitialConditionOfNetwork,
                InitialAssetSummaries = simulationOutput.InitialAssetSummaries.Select(_ => _.ToEntity()),
                Years = simulationOutput.Years.Select(_ => _.ToEntity())
            };
        }
    }
}
