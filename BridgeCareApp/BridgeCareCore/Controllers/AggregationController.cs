using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.Analysis;
using AppliedResearchAssociates.iAM.Common;
using AppliedResearchAssociates.iAM.Data;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.Hubs;
using AppliedResearchAssociates.iAM.Hubs.Interfaces;
using BridgeCareCore.Controllers.BaseController;
using BridgeCareCore.Interfaces;
using BridgeCareCore.Models;
using BridgeCareCore.Security;
using BridgeCareCore.Security.Interfaces;
using BridgeCareCore.Services;
using BridgeCareCore.Services.Aggregation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BridgeCareCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AggregationController : BridgeCareCoreBaseController
    {
        public const string AggregationError = "Aggregation Error";
        private readonly ILog _log;
        private readonly IAggregationService _aggregationService;
        private readonly IGeneralWorkQueueService _generalWorkQueueService;


        public AggregationController(ILog log, IAggregationService aggregationService,
            IEsecSecurity esecSecurity, UnitOfDataPersistenceWork unitOfWork,
            IHubService hubService, IHttpContextAccessor httpContextAccessor, IGeneralWorkQueueService generalWorkQueueService) :
            base(esecSecurity, unitOfWork, hubService, httpContextAccessor)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _aggregationService = aggregationService ?? throw new ArgumentNullException(nameof(aggregationService));
            _generalWorkQueueService = generalWorkQueueService ?? throw new ArgumentNullException(nameof(generalWorkQueueService));
        }

        [HttpPost]
        [Route("AggregateNetworkData/{networkId}")]
        [ClaimAuthorize("NetworkAggregateAccess")]
        public async Task<IActionResult> AggregateNetworkData(Guid networkId, List<AllAttributeDTO> attributes)
        {
            try
            {
                var networkName = "";
                var specificAttributes = new List<AttributeDTO>();
                await Task.Factory.StartNew(() =>
                {
                    specificAttributes = AttributeService.ConvertAllAttributeList(attributes);
                    networkName = UnitOfWork.NetworkRepo.GetNetworkName(networkId);
                });
                AggregationWorkitem workItem = new AggregationWorkitem(networkId, UserInfo.Name, networkName, specificAttributes);
                var analysisHandle = _generalWorkQueueService.CreateAndRun(workItem);

                Debug.WriteLine($"Aggregation started at {DateTime.Now}");
                HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastWorkQueueUpdate, networkId.ToString());

                return Ok();
            }
            catch (UnauthorizedAccessException e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name,  $"{AggregationError}::NetworkAggregateAccess - {HubService.errorList["Unauthorized"]}", e);
                return Ok();
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{AggregationError}::NetworkAggregateAccess - {e.Message}", e);
                return Ok();
            }
        }

    }
}
