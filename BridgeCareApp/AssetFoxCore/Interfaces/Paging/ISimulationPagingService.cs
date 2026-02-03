using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;
using AssetFoxCore.Models;

namespace AssetFoxCore.Interfaces
{
    public interface ISimulationPagingService
    {
        PagingPageModel<SimulationDTO> GetUserScenarioPage(PagingRequestModel<SimulationDTO> request);

        public PagingPageModel<SimulationDTO> GetSharedScenarioPage(PagingRequestModel<SimulationDTO> request, bool hasAdminAccess, bool hasSimulationAccess);
    }
}
