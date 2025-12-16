using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.Hubs.Interfaces;
using AssetFoxCore.Controllers.BaseController;
using AssetFoxCore.Models;
using AssetFoxCore.Security.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using HotChocolate.Authorization;
using AssetFox.Core.DTOs;
using System.Collections.Generic;
using Microsoft.SqlServer.Dac.Model;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Services;

namespace AssetFoxCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataSourceMappingController : AssetFoxCoreBaseController
    {
        public const string DataSourceMappingError = "DataSourceMapping Error";
        private IDataSourceMappingService _dataSourceMappingService;

        public DataSourceMappingController(
            IDataSourceMappingService dataSourceMappingService,
            IEsecSecurity esecSecurity,
            IUnitOfWork unitOfWork,
            IHubService hubService,
            IHttpContextAccessor contextAccessor)
            : base(esecSecurity,
                  unitOfWork,
                  hubService,
                  contextAccessor)
        {
            _dataSourceMappingService = dataSourceMappingService ?? throw new ArgumentNullException(nameof(DataSourceMappingService));
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

        [HttpGet]
        [Route("DownloadDataSourceMappings/{dataSourceId}")]
        [Authorize]
        public async Task<IActionResult> DownloadDataSourceMappings(Guid dataSourceId)
        {
            try
            {
                var result = await Task.Factory.StartNew(() =>
                {
                    return _dataSourceMappingService.DownloadDataSourceMappings(dataSourceId);
                });

                return Ok(result);
            }            
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{DataSourceMappingError}::DownloadDataSourceMappings - {e.Message}", e);
            }
            return Ok();
        }
    }
}
