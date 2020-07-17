using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Filters;
using BridgeCare.Interfaces.ConditionResults;
using BridgeCare.Interfaces.ReportsDownload;
using BridgeCare.Models;
using BridgeCare.Security;
using BridgeCare.Services.ConditionResultReport;
using Hangfire;

namespace BridgeCare.Controllers
{
    public class ConditionResultReportController : ApiController
    {
        private readonly IConditionResultReportGenerator conditionResultReportGenerator;
        private readonly IReportsDownload<ConditionResultReportGenerator> conditionReportDownload;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ConditionResultReportController));

        public ConditionResultReportController(
            IConditionResultReportGenerator conditionResultReportGenerator,
            IReportsDownload<ConditionResultReportGenerator> conditionReportDownload)
        {
            this.conditionResultReportGenerator = conditionResultReportGenerator;
            this.conditionReportDownload = conditionReportDownload;
        }
        /// <summary>
        /// API endpoint for fetching data for Condition results report
        /// </summary>
        /// <param name="model">SimulationModel</param>
        /// <returns>IHttpActionResult</returns>
        [HttpPost]
        [Route("api/GenerateConditionResultReport")]
        [ModelValidation("The scenario data is invalid.")]
        [RestrictAccess]
        public HttpResponseMessage GenerateConditionResultReport([FromBody] SimulationModel model)
        {
            BackgroundJob.Enqueue(() => conditionResultReportGenerator.GenerateConditionResultReport(model));
            var response = Request.CreateResponse(HttpStatusCode.OK, "Report generation started");
            return response;
        }

        [HttpPost]
        [Route("api/DownloadConditionResultReport")]
        [ModelValidation("The scenario data is invalid.")]
        [RestrictAccess]
        public HttpResponseMessage DownloadConditionResultReport([FromBody] SimulationModel model)
        {
            var folderPath = $"DownloadedReports\\ConditionResult\\{model.simulationId}";
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderPath, "ConditionResultReport.xlsx");
            var response = new HttpResponseMessage();
            if (!File.Exists(filePath))
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, $"condition result report is not available in the path {filePath}");
                log.Error($"condition result report is not available in the path {filePath}");
                return response;
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new ByteArrayContent(conditionReportDownload.DownloadExcelReport(model));
            }
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "ConditionResultReport.xlsx"
            };
            return response;
        }
    }
}
