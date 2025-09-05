using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using BridgeCareCore.Controllers.BaseController;
using BridgeCareCore.Security.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.Hubs;
using AppliedResearchAssociates.iAM.Hubs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BridgeCareCore.Services;
using System.Data;
using OfficeOpenXml;


namespace BridgeCareCore.Controllers
{

    [Route("api/[controller]")]
    public class RawDataController : BridgeCareCoreBaseController
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
                    // TODO On upload of file to DS mapping table entries to be saved
                    var mappingsResult = await Task.Factory.StartNew(() =>
                    {
                        var mappingsWorksheet = excelPackage.Workbook.Worksheets[1]; // TODO test if its not present                        
                        return _excelSpreadsheetImportService.ImportDataSourceMapping(dataSourceId, worksheet, mappingsWorksheet);
                    });
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
