using System;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.Common;
using System.Threading;
using AssetFox.Core.Common.Logging;
using AssetFox.Core.DTOs;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface ISimulationOutputRepository
    {
        void CreateSimulationOutputViaRelational(Guid simulationId, SimulationOutput simulationOutput,
            IWorkQueueLog logerForUserInfo = null, ILog loggerForTechnicalInfo = null, CancellationToken? cancellationToken = null);

        SimulationOutput GetSimulationOutputViaRelation(Guid simulationId, ILog loggerForUserInfo = null, ILog loggerForTechincalInfo = null, List<AttributeDTO> attributeDtos = null);

        SimulationOutputEntity GetSimulationOutputWithoutAssetSummariesOrYearContents(Guid simulationId);
                
        SimulationOutput GetSimulationOutputViaJson(Guid simulationId);

        void CreateSimulationOutputViaJson(Guid simulationId, SimulationOutput simulationOutput);

        void ConvertSimulationOutpuFromJsonTorelational(Guid simulationId, CancellationToken? cancellationToken = null, IWorkQueueLog queueLogger = null);

        void DeleteScenarioOutputsWithingDaterange(DateTime? startDate, DateTime endDate, CancellationToken token);

        SimulationOutputDTO GetSimulationOutput(Guid simulationId);

        void CreateSimulationOutputRelational(SimulationOutputEntity simulationOutputEntity);
    }
}
