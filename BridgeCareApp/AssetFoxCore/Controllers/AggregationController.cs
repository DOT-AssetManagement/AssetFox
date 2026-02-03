using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using AssetFox.Core.Analysis;
using AssetFox.Core.Common;
using AssetFox.Core.Data;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFoxCore.Controllers.BaseController;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Models;
using AssetFoxCore.Security;
using AssetFoxCore.Security.Interfaces;
using AssetFoxCore.Services;
using AssetFoxCore.Services.Aggregation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AssetFoxCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AggregationController : AssetFoxCoreBaseController
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
        [Route("AggregateNetworkData/{dataSource}/{networkId}")]
        [ClaimAuthorize("NetworkAggregateAccess")]
        public async Task<IActionResult> AggregateNetworkData(Guid networkId, Guid dataSource)
        {
            try
            {
                var networkName = "";
                await Task.Factory.StartNew(() =>
                {
                    networkName = UnitOfWork.NetworkRepo.GetNetworkName(networkId);
                });
                AggregationWorkitem workItem = new AggregationWorkitem(networkId, UserInfo.Name, networkName, dataSource);
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
