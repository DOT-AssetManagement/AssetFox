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

        public SimulationOutput GetSimulationOutputSimpleViaRelation(Guid simulationId,
            List<SimulationYearDetailEntity> cacheYears,
            Dictionary<Guid, string> attributeNameLookup,
            SimulationOutputEntity entityWithoutAssetSummariesOrYearContents,
            ILog loggerForUserInfo = null,
            ILog loggerForTechinalInfo = null);

        public List<AssetSummaryDetail> GetSimulationOutputInitialAssetSummariesViaRelation(Guid simulationOutputId,
            Dictionary<Guid, string> attributeNameLookup,
            Dictionary<Guid, string> assetNameLookup,
            ILog loggerForUserInfo = null,
            ILog loggerForTechinalInfo = null);

        public List<SimulationYearDetail> GetSimulationOutputYearsViaRelation(Guid simulationId,
            List<SimulationYearDetailEntity> cacheYears,
            Dictionary<Guid, string> attributeNameLookup,
            Dictionary<Guid, string> assetNameLookup,
            ILog loggerForUserInfo = null,
            ILog loggerForTechinalInfo = null);        

        SimulationOutput GetSimulationOutputViaJson(Guid simulationId);
        void CreateSimulationOutputViaJson(Guid simulationId, SimulationOutput simulationOutput);

        void ConvertSimulationOutpuFromJsonTorelational(Guid simulationId, CancellationToken? cancellationToken = null, IWorkQueueLog queueLogger = null);
        void DeleteScenarioOutputsWithingDaterange(DateTime? startDate, DateTime endDate, CancellationToken token);
    }
}
