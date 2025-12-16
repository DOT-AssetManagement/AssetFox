using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs;
using AssetFox.Core.Common.Logging;
using System.Threading;
using AssetFox.Core.Common;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface ISimulationRepository
    {
        void GetSimulationInNetwork(Guid simulationId, Network network);

        List<SimulationDTO> GetAllScenario();



        List<SimulationDTO> GetUserScenarios();



        List<SimulationDTO> GetSharedScenarios(bool hasAdminAccess, bool hasSimulationAccess);



        List<SimulationDTO> GetScenariosWithIds(List<Guid> simulationIds);

        void CreateSimulation(Guid networkId, SimulationDTO dto);

        SimulationDTO GetSimulation(Guid simulationId);
                
        /// <summary>Updates the simulation. If the dto has a nonempty

        /// list of users, also updates the users. BUT if the

        /// dto's list of users is empty, the users are unaffected.</summary> 
        void UpdateSimulationAndPossiblyUsers(SimulationDTO dto);

        void DeleteSimulation(Guid simulationId, CancellationToken? cancellationToken = null, IWorkQueueLog queueLog = null);
        void DeleteSimulationReports(Guid simulationId);

        void DeleteSimulationsByNetworkId(Guid networkId);

        void UpdateLastModifiedDate(SimulationEntity entity);

        string GetSimulationName(Guid simulationId);

        List<string> GetAllSimulationNames();

        bool GetSimulationRunSetting(Guid simulationId);

        List<SimulationAnalysisDetailEntity> GetScenariosReportSettings();

        SimulationDTO GetCurrentUserOrSharedScenario(Guid simulationId, bool hasAdminAccess, bool hasSimulationAccess);

        

        bool GetNoTreatmentBeforeCommitted(Guid simulationId);



        void SetNoTreatmentBeforeCommitted(Guid simulationId);



        void RemoveNoTreatmentBeforeCommitted(Guid simulationId);

        SimulationCloningResultDTO CreateSimulation(CompleteSimulationDTO completeSimulationDTO, string keyAttribute, SimulationCloningCommittedProjectErrors simulationCloningCommittedProjectErrors, BaseEntityProperties baseEntityProperties);

    }
}
