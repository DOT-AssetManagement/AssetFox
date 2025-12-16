using System;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Reporting;
using AssetFoxCore.Controllers.BaseController;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFoxCore.Security.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using AssetFox.Core.DataPersistenceCore.Repositories;

namespace AssetFoxCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimulationLogController : AssetFoxCoreBaseController
    {
        public SimulationLogController(
            IEsecSecurity esecSecurity,
            UnitOfDataPersistenceWork unitOfWork,
            IHubService hubService,
            IHttpContextAccessor httpContextAccessor) :
            base(esecSecurity, unitOfWork, hubService, httpContextAccessor)
        {
            
        }
            

        [HttpPost]
        [Route("GetSimulationLog/{networkId}/{simulationId}")]
        [Authorize]
        public async Task<FileResult> GetSimulationLog(Guid networkId, Guid simulationId)
        {

            var reportDetailDto = new SimulationReportDetailDTO { SimulationId = simulationId, Status = "Generating", ReportType = "SimulationLog" };
            var simulationName = UnitOfWork.SimulationRepo.GetSimulationNameOrId(simulationId);

            try
            {
                UpdateSimulationAnalysisDetail(reportDetailDto);
                HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastReportGenerationStatus, reportDetailDto);

                var logDtos = await UnitOfWork.SimulationLogRepo.GetLog(simulationId);
                var log = SimulationLogReport.ToLog(logDtos);
                var bytes = Encoding.Unicode.GetBytes(log);
                const string contentType = "text/plain";
                HttpContext.Response.ContentType = contentType;
                HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

                var fileContentResult = new FileContentResult(bytes, contentType)
                {
                    FileDownloadName = "SimulationLog.txt"
                };

                reportDetailDto.Status = "Completed";
                UpdateSimulationAnalysisDetail(reportDetailDto);
                HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastReportGenerationStatus, reportDetailDto);

                return fileContentResult;
            }
            catch (Exception e)
            {
                reportDetailDto.Status = $"Failed to generate";
                UpdateSimulationAnalysisDetail(reportDetailDto);
                HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastReportGenerationStatus, reportDetailDto);
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"Summary Report Error::GetSimulationLog for {simulationName} - {e.Message}", e);
            }
            return null;
        }

        private void UpdateSimulationAnalysisDetail(SimulationReportDetailDTO dto) =>
            UnitOfWork.SimulationReportDetailRepo.UpsertSimulationReportDetail(dto);
    }
}
