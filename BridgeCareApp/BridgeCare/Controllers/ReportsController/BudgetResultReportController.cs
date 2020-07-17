using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Filters;
using BridgeCare.Interfaces.BudgetResults;
using BridgeCare.Interfaces.ReportsDownload;
using BridgeCare.Models;
using BridgeCare.Security;
using BridgeCare.Services.BudgetResultsReport;
using Hangfire;

namespace BridgeCare.Controllers
{
    public class BudgetResultReportController : ApiController
    {
        private readonly IBudgetResultReportGenerator budgetResultReportGenerator;
        private readonly IReportsDownload<BudgetResultsReportGenerator> budgetReportDownload;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(BudgetResultReportController));

        public BudgetResultReportController(
            IBudgetResultReportGenerator budgetResultReportGenerator,
            IReportsDownload<BudgetResultsReportGenerator> budgetReportDownload)
        {
            this.budgetResultReportGenerator = budgetResultReportGenerator;
            this.budgetReportDownload = budgetReportDownload;
        }
        [HttpPost]
        [Route("api/GenerateBudgetResultReport")]
        [ModelValidation("The scenario data is invalid.")]
        [RestrictAccess]
        public HttpResponseMessage GenerateBudgetResultsReport([FromBody] SimulationModel model)
        {
            BackgroundJob.Enqueue(() => budgetResultReportGenerator.GenerateBudgetResultReport(model));
            var response = Request.CreateResponse(HttpStatusCode.OK, "Report generation started");
            return response;
        }

        [HttpPost]
        [Route("api/DownloadBudgetResultReport")]
        [ModelValidation("The scenario data is invalid.")]
        [RestrictAccess]
        public HttpResponseMessage DownloadBudgetResultReport([FromBody] SimulationModel model)
        {
            var folderPath = $"DownloadedReports\\BudgetResult\\{model.simulationId}";
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderPath, "BudgetResultsReport.xlsx");
            var response = new HttpResponseMessage();
            if (!File.Exists(filePath))
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, $"budget results report is not available in the path {filePath}");
                log.Error($"budget result report is not available in the path {filePath}");
                return response;
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new ByteArrayContent(budgetReportDownload.DownloadExcelReport(model));
            }
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "BudgetResultsReport.xlsx"
            };
            return response;
        }
    }
}
