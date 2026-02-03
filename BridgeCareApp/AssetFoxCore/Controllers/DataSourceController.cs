using System;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Abstract;
using AssetFox.Core.DTOs.Enums;
using AssetFoxCore.Controllers.BaseController;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFoxCore.Models;
using AssetFoxCore.Models.Validation;
using AssetFoxCore.Security.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Linq;
using AssetFoxCore.Security;
using AssetFox.Core.Hubs.Services;

namespace AssetFoxCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataSourceController : AssetFoxCoreBaseController
    {
        public const string DataSourceError = "DataSource Error";

        public DataSourceController(
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

        [HttpPost]
        [Route("UpsertSqlDataSource")]
        [ClaimAuthorize("DataSourceModifyAccess")]
        public async Task<IActionResult> UpsertSqlDataSource(SQLDataSourceDTO dto)
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    UnitOfWork.DataSourceRepo.UpsertDatasource(dto);
                });
                return Ok();
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{DataSourceError}::UpsertSqlDataSource - {e.Message}", e);
            }
            return Ok();
        }

        [HttpPost]
        [Route("UpsertExcelDataSource")]
        [ClaimAuthorize("DataSourceModifyAccess")]
        public async Task<IActionResult> UpsertExcelDataSource(ExcelDataSourceDTO dto)
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    UnitOfWork.DataSourceRepo.UpsertDatasource(dto);
                });
                return Ok();
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{DataSourceError}::UpsertExcelDataSource - {e.Message}", e);
            }
            return Ok();
        }
        [HttpDelete]
        [Route("DeleteDataSource/{dataSourceId}")]
        [ClaimAuthorize("DataSourceModifyAccess")]
        public async Task<IActionResult> DeleteDataSource(Guid dataSourceId)
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    UnitOfWork.DataSourceRepo.DeleteDataSource(dataSourceId);
                });

                return Ok();
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{DataSourceError}::DeleteDataSource - {e.Message}", e);
            }
            return Ok();
        }

        [HttpGet]
        [Route("GetDataSources")]
        [ClaimAuthorize("DataSourceViewAccess")]
        public async Task<IActionResult> GetDataSources()
        {
            try
            {
                var result = await Task.Factory.StartNew(() => UnitOfWork.DataSourceRepo.GetDataSources());
                return Ok(result);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{DataSourceError}::GetDataSources - {e.Message}", e);
            }
            return Ok();
        }

        [HttpGet]
        [Route("GetDataSource/{dataSourceId}")]
        [ClaimAuthorize("DataSourceViewAccess")]
        public async Task<IActionResult> GetDataSource(Guid dataSourceId)
        {
            try
            {
                var result = await Task.Factory.StartNew(() => UnitOfWork.DataSourceRepo.GetDataSource(dataSourceId));
                if (result is SQLDataSourceDTO)
                {
                    return Ok((SQLDataSourceDTO)result);
                }
                else if (result is ExcelDataSourceDTO)
                {
                    return Ok((ExcelDataSourceDTO)result);
                }
                return Ok(result);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name,  $"{DataSourceError}::GetDataSource - {e.Message}", e);
            }
            return Ok();
        }

        [HttpGet]
        [Route("GetDataSourceTypes")]
        [ClaimAuthorize("DataSourceViewAccess")]
        public Task<IActionResult> GetDataSourceTypes()
        {
            try
            {
                var dataSourceArray = (DataSourceTypeStrings[])Enum.GetValues(typeof(DataSourceTypeStrings));
                //All and None are internal data types we do not want to expose to the UI
                var result = dataSourceArray.Where(q => q.ToString() != "All" && q.ToString() != "None")
                    .Select(v => v.ToString()).ToList();
                return Task.FromResult<IActionResult>(Ok(result));
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"Treatment Error::GetDataSourceTypes - {e.Message}", e);
                throw;
            }
        }

        [HttpPost]
        [Route("CheckSqlConnection")]
        [ClaimAuthorize("DataSourceViewAccess")]
        public async Task<IActionResult> CheckSqlConnection(TestStringData stringData)
        {
            try
            {
                try
                {
                    await Task.Factory.StartNew(() =>
                    {
                        using (SqlConnection conn = new SqlConnection(stringData.testString))
                        {
                            conn.Open(); // throws if invalid
                        }
                    });
                    return Ok(new ValidationResult() { IsValid = true, ValidationMessage = "Connection string is valid"});
                }
                catch (Exception ex)
                {
                    return Ok(new ValidationResult() { IsValid = false, ValidationMessage = "Connection string is not valid: " + ex.Message });
                }

            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"Attribute Error::CheckSqlConnection - {e.Message}", e);
            }
            return Ok();
        }
    }
}
