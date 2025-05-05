using System;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.Common;
using System.Threading;
using AppliedResearchAssociates.iAM.Common.Logging;
using AppliedResearchAssociates.iAM.DTOs;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories
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
