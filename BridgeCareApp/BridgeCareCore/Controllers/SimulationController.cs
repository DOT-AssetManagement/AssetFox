using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.DTOs.Static;
using BridgeCareCore.Controllers.BaseController;
using BridgeCareCore.Hubs;
using BridgeCareCore.Interfaces;
using BridgeCareCore.Security;
using BridgeCareCore.Security.Interfaces;
using BridgeCareCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BridgeCareCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimulationController : BridgeCareCoreBaseController
    {
        private readonly ISimulationAnalysis _simulationAnalysis;

        private readonly IReadOnlyDictionary<string, SimulationCRUDMethods> _simulationCRUDMethods;

        public SimulationController(ISimulationAnalysis simulationAnalysis, IEsecSecurity esecSecurity, UnitOfDataPersistenceWork unitOfWork,
            IHubService hubService, IHttpContextAccessor httpContextAccessor) : base(esecSecurity, unitOfWork, hubService, httpContextAccessor)
        {
            _simulationAnalysis = simulationAnalysis ?? throw new ArgumentNullException(nameof(simulationAnalysis));
            _simulationCRUDMethods = CreateCRUDMethods();
        }

        private Dictionary<string, SimulationCRUDMethods> CreateCRUDMethods()
        {
            void UpdateAnySimulation(SimulationDTO dto) => UnitOfWork.SimulationRepo.UpdateSimulation(dto);

            void UpdatePermittedSimulation(SimulationDTO dto)
            {
                CheckUserSimulationModifyAuthorization(dto.Id);
                UpdateAnySimulation(dto);
            }

            void DeleteAnySimulation(Guid simulationId) => UnitOfWork.SimulationRepo.DeleteSimulation(simulationId);

            void DeletePermittedSimulation(Guid simulationId)
            {
                CheckUserSimulationModifyAuthorization(simulationId);
                DeleteAnySimulation(simulationId);
            }

            IQueuedWorkHandle RunAnySimulation(Guid networkId, Guid simulationId) =>
                _simulationAnalysis.CreateAndRun(networkId, simulationId);

            IQueuedWorkHandle RunPermittedSimulation(Guid networkId, Guid simulationId)
            {
                CheckUserSimulationModifyAuthorization(simulationId);
                return RunAnySimulation(networkId, simulationId);
            }

            List<SimulationDTO> RetrieveAllSimulations(Guid networkId) => UnitOfWork.SimulationRepo.GetAllInNetwork(networkId);

            List<SimulationDTO> RetrievePermittedSimulations(Guid networkId)
            {
                // TODO:  Replace with query to find all shared simulations
                return RetrieveAllSimulations(networkId);
            }

            // TODO: Add another 2 methods in to controll simulation cloning (the user should only be able to clone scenarios that they have access to)

            var AdminCRUDMethods = new SimulationCRUDMethods()
            {
                UpsertSimulation = UpdateAnySimulation,
                RetrieveSimulation = RetrieveAllSimulations,
                DeleteSimulation = DeleteAnySimulation,
                RunSimulation = RunAnySimulation
            };

            var PermittedCRUDMethods = new SimulationCRUDMethods()
            {
                UpsertSimulation = UpdatePermittedSimulation,
                RetrieveSimulation = RetrievePermittedSimulations,
                DeleteSimulation = DeletePermittedSimulation,
                RunSimulation = RunPermittedSimulation
            };

            return new Dictionary<string, SimulationCRUDMethods>
            {
                [Role.Administrator] = AdminCRUDMethods,
                [Role.DistrictEngineer] = PermittedCRUDMethods,
                [Role.Cwopa] = PermittedCRUDMethods,
                [Role.PlanningPartner] = PermittedCRUDMethods
            };
        }

        [HttpGet]
        [Route("GetScenarios/{networkId}")]
        [RestrictAccess]
        public async Task<IActionResult> GetSimulations(Guid networkId)
        {
            try
            {
                var result = await Task.Factory.StartNew(() => _simulationCRUDMethods[UserInfo.Role].RetrieveSimulation(networkId));
                return Ok(result);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastError, $"Scenario error::{e.Message}");
                throw;
            }
        }

        [HttpPost]
        [Route("CreateScenario/{networkId}")]
        [RestrictAccess]
        public async Task<IActionResult> CreateSimulation(Guid networkId, [FromBody] SimulationDTO dto)
        {
            try
            {
                var result = await Task.Factory.StartNew(() =>
                {
                    UnitOfWork.BeginTransaction();
                    UnitOfWork.SimulationRepo.CreateSimulation(networkId, dto);
                    UnitOfWork.Commit();
                    return UnitOfWork.SimulationRepo.GetSimulation(dto.Id);
                });

                return Ok(result);
            }
            catch (Exception e)
            {
                UnitOfWork.Rollback();
                HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastError, $"Scenario error::{e.Message}");
                throw;
            }
        }

        [HttpPost]
        [Route("CloneScenario/{simulationId}")]
        [RestrictAccess]
        public async Task<IActionResult> CloneSimulation(Guid simulationId)
        {
            try
            {
                var result = await Task.Factory.StartNew(() =>
                {
                    UnitOfWork.BeginTransaction();
                    var cloneResult = UnitOfWork.SimulationRepo.CloneSimulation(simulationId);
                    UnitOfWork.Commit();
                    return cloneResult;
                });

                if (result.WarningMessage != null)
                {
                    HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastWarning, result.WarningMessage);
                }
                return Ok(result.Simulation);
            }
            catch (Exception e)
            {
                UnitOfWork.Rollback();
                HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastError, $"Scenario error::{e.Message}");
                throw;
            }
        }

        [HttpPut]
        [Route("UpdateScenario")]
        [RestrictAccess]
        public async Task<IActionResult> UpdateSimulation([FromBody] SimulationDTO dto)
        {
            try
            {
                var result = await Task.Factory.StartNew(() =>
                {
                    UnitOfWork.BeginTransaction();
                    _simulationCRUDMethods[UserInfo.Role].UpsertSimulation(dto);
                    UnitOfWork.Commit();
                    return UnitOfWork.SimulationRepo.GetSimulation(dto.Id);
                });

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                UnitOfWork.Rollback();
                return Unauthorized();
            }
            catch (Exception e)
            {
                UnitOfWork.Rollback();
                HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastError, $"Scenario error::{e.Message}");
                throw;
            }
        }

        [HttpDelete]
        [Route("DeleteScenario/{simulationId}")]
        [RestrictAccess]
        public async Task<IActionResult> DeleteSimulation(Guid simulationId)
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    UnitOfWork.BeginTransaction();
                    _simulationCRUDMethods[UserInfo.Role].DeleteSimulation(simulationId);
                    UnitOfWork.Commit();
                });

                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                UnitOfWork.Rollback();
                return Unauthorized();
            }
            catch (Exception e)
            {
                UnitOfWork.Rollback();
                HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastError, $"Scenario error::{e.Message}");
                throw;
            }
        }

        [HttpPost]
        [Route("RunSimulation/{networkId}/{simulationId}")]
        [RestrictAccess]
        public async Task<IActionResult> RunSimulation(Guid networkId, Guid simulationId)
        {
            try
            {
                var analysisHandle = _simulationCRUDMethods[UserInfo.Role].RunSimulation(networkId, simulationId);
                // Before sending a "queued" message that may overwrite early messages from the run,
                // allow a brief moment for an empty queue to start running the submission.
                await Task.Delay(500);
                if (!analysisHandle.WorkHasStarted)
                {
                    var simulationAnalysisDetail = new SimulationAnalysisDetailDTO
                    {
                        SimulationId = simulationId,
                        LastRun = DateTime.Now,
                        Status = "Queued to run."
                    };
                    HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastSimulationAnalysisDetail, simulationAnalysisDetail);
                }
                await analysisHandle.WorkCompletion;
                return Ok();
            }
            catch (Exception e)
            {
                HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastError, $"Scenario error::{e.Message}");
                if (!(e is SimulationException))
                {
                    var logDto = SimulationLogDtos.GenericException(simulationId, e);
                    UnitOfWork.SimulationLogRepo.CreateLog(logDto);
                }
                throw;
            }
        }
    }

    internal class SimulationCRUDMethods
    {
        public Action<SimulationDTO> UpsertSimulation { get; set; }
        public Func<Guid, List<SimulationDTO>> RetrieveSimulation { get; set; }
        public Action<Guid> DeleteSimulation { get; set; }
        public Func<Guid, Guid, IQueuedWorkHandle> RunSimulation { get; set; }
    }
}
