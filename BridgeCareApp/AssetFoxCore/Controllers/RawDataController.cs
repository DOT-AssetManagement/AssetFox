using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFoxCore.Controllers.BaseController;
using AssetFoxCore.Security.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AssetFoxCore.Services;
using System.Data;
using OfficeOpenXml;


namespace AssetFoxCore.Controllers
{

    [Route("api/[controller]")]
    public class RawDataController : AssetFoxCoreBaseController
    {
        public const string RawDataError = "Raw Data Error";

        private readonly IExcelRawDataImportService _excelSpreadsheetImportService;
        private readonly IExcelRawDataLoadService _excelRawDataLoadService;
        public RawDataController(
            IEsecSecurity esecSecurity,
            IUnitOfWork unitOfWork,
            IHubService hubService,
            IHttpContextAccessor contextAccessor,
            IExcelRawDataImportService excelSpreadsheetImportService,
            IExcelRawDataLoadService excelRawDataLoadService
            ) : base(esecSecurity, unitOfWork, hubService, contextAccessor)
        {
            _excelSpreadsheetImportService = excelSpreadsheetImportService;
            _excelRawDataLoadService = excelRawDataLoadService;
        }

        [HttpPost]
        [Route("ImportExcelSpreadsheet/{dataSourceId}")]
        [RequestSizeLimit(104857600)]
        [Authorize]
        public async Task<IActionResult> ImportExcelSpreadsheet(
            Guid dataSourceId)
        {
            try
            {
                if (!ContextAccessor.HttpContext.Request.HasFormContentType)
                {
                    throw new ConstraintException("Request MIME type is invalid.");
                }

                if (ContextAccessor.HttpContext.Request.Form.Files.Count < 1)
                {
                    throw new ConstraintException("Attributes file not found.");
                }

                var excelPackage = new ExcelPackage(ContextAccessor.HttpContext.Request.Form.Files[0].OpenReadStream());
                var worksheet = excelPackage.Workbook.Worksheets[0];

                var result = await Task.Factory.StartNew(() =>
                {
                    return _excelSpreadsheetImportService.ImportRawData(dataSourceId, worksheet);
                });

                if (result.WarningMessage != null)
                {
                    HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastWarning, result.WarningMessage);
                }
                else
                {
                    var mappingsResult = await Task.Factory.StartNew(() =>
                    {
                        var mappingsWorksheet = excelPackage.Workbook.Worksheets.Count > 1 ? excelPackage.Workbook.Worksheets[1] : null;
                        return _excelSpreadsheetImportService.ImportDataSourceMapping(dataSourceId, worksheet, mappingsWorksheet);
                    });

                    if (mappingsResult.WarningMessage != null)
                    {
                        HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastWarning, mappingsResult.WarningMessage);
                    }
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{RawDataError}::ImportExcelSpreadsheet - {e.Message}", e);
            }
            return Ok();
        }
        [HttpGet]
        [Route("GetExcelSpreadsheetColumnHeaders/{dataSourceId}")]
        [Authorize]
        public async Task<IActionResult> GetExcelSpreadsheetColumnHeaders(Guid dataSourceId)
        {
            try
            {
                var result = await Task.Factory.StartNew(() =>
                {
                    return _excelRawDataLoadService.GetSpreadsheetColumnHeaders(dataSourceId);
                });

                if (result.WarningMessage != null)
                {
                    HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastWarning, result.WarningMessage);
                }
                return Ok(result);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{RawDataError}::GetExcelSpreadsheetColumnHeaders - {e.Message}", e);
            }
            return Ok();
        }

    }
}
