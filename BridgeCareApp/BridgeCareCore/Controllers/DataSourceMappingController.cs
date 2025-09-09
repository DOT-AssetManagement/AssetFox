using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.Hubs.Interfaces;
using BridgeCareCore.Controllers.BaseController;
using BridgeCareCore.Models;
using BridgeCareCore.Security.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using HotChocolate.Authorization;
using AppliedResearchAssociates.iAM.DTOs;
using BridgeCareCore.Security;
using Microsoft.Graph.Models;
using System.Collections.Generic;

namespace BridgeCareCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataSourceMappingController : BridgeCareCoreBaseController
    {
        public const string DataSourceMappingError = "DataSourceMapping Error";

        public DataSourceMappingController(
            IEsecSecurity esecSecurity,
            IUnitOfWork unitOfWork,
            IHubService hubService,
            IHttpContextAccessor contextAccessor)
            : base(esecSecurity,
                  unitOfWork,
                  hubService,
                  contextAccessor)
        {
        }        

        [HttpGet]
        [Route("GetDataSourceMappings/{dataSourceId}")]
        [Authorize]
        public async Task<IActionResult> GetDataSourceMappings(Guid dataSourceId)
        {
            try
            {
                var result = await Task.Factory.StartNew(() => UnitOfWork.DataSourceMappingRepo.GetDataSourceMappings(dataSourceId));

                return Ok(result);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{DataSourceMappingError}::GetDataSourceMappings - {e.Message}", e);
            }
            return Ok();
        }
                
        [HttpPost]
        [Route("UpsertDataSourceMappings/{dataSourceId}")]
        [Authorize]
        public async Task<IActionResult> UpsertDataSourceMappings(Guid dataSourceId, List<DataSourceMappingDTO> dtos)
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    UnitOfWork.DataSourceMappingRepo.UpsertDataSourceMappings(dtos, dataSourceId);
                });
                return Ok();
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{DataSourceMappingError}::UpsertDataSourceMappings - {e.Message}", e);
            }
            return Ok();
        }
    }
}
