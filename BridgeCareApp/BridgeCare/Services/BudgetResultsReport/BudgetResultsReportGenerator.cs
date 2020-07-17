using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using BridgeCare.Interfaces;
using BridgeCare.Interfaces.BudgetResults;
using BridgeCare.Interfaces.ReportsDownload;
using BridgeCare.Models;
using BridgeCare.Models.SummaryReport.ParametersTAB;
using BridgeCare.Properties;
using BridgeCare.Services.CommonData;
using Hangfire;
using MongoDB.Driver;
using OfficeOpenXml;

namespace BridgeCare.Services.BudgetResultsReport
{
    public class BudgetResultsReportGenerator : IBudgetResultReportGenerator, IReportsDownload<BudgetResultsReportGenerator>
    {
        private readonly ICommonSummaryReportData commonSummaryReportData;
        private readonly BudgetResultsDataTAB budgetResultsDataTAB;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(BudgetResultsReportGenerator));

        public BudgetResultsReportGenerator(ICommonSummaryReportData commonSummaryReportData, BudgetResultsDataTAB budgetResultsDataTAB)
        {
            this.commonSummaryReportData = commonSummaryReportData ??
                throw new ArgumentNullException(nameof(commonSummaryReportData));
            this.budgetResultsDataTAB = budgetResultsDataTAB ??
                throw new ArgumentNullException(nameof(budgetResultsDataTAB));
        }

        [AutomaticRetry(Attempts = 0)]
        public void GenerateBudgetResultReport(SimulationModel simulationModel)
        {
            // Get data
            var simulationId = simulationModel.simulationId;
            var simulationYearsModel = commonSummaryReportData.GetSimulationYearsData(simulationId);
            var simulationYears = simulationYearsModel.Years;
            simulationYears.Sort();
            var simulationYearsCount = simulationYears.Count;

            using (var excelPackage = new ExcelPackage(new System.IO.FileInfo("BudgetResultsReport.xlsx")))
            {
#if DEBUG
                var mongoConnection = Settings.Default.MongoDBDevConnectionString;
#else
                var mongoConnection = Settings.Default.MongoDBProdConnectionString;
#endif
                var client = new MongoClient(mongoConnection);
                var MongoDatabase = client.GetDatabase("BridgeCare");
                var simulations = MongoDatabase.GetCollection<SimulationModel>("scenarios");

                var updateStatus = Builders<SimulationModel>.Update
                    .Set(s => s.status, "Begin budget result report generation");
                simulations.UpdateOne(s => s.simulationId == simulationId, updateStatus);

                var worksheet = excelPackage.Workbook.Worksheets.Add("Bridge Data");
                budgetResultsDataTAB.Fill(worksheet, simulationModel, simulationYears);

                var folderPathForSimulation = $"DownloadedReports\\BudgetResult\\{simulationModel.simulationId}";
                string relativeFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderPathForSimulation);
                Directory.CreateDirectory(relativeFolderPath);
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderPathForSimulation, "BudgetResultsReport.xlsx");
                byte[] bin = excelPackage.GetAsByteArray();
                File.WriteAllBytes(filePath, bin);

                updateStatus = Builders<SimulationModel>.Update
                    .Set(s => s.status, "Budget result report has been generated");
                simulations.UpdateOne(s => s.simulationId == simulationId, updateStatus);
            }
        }

        public byte[] DownloadExcelReport(SimulationModel simulationModel)
        {
            var folderPathForSimulation = $"DownloadedReports\\BudgetResult\\{simulationModel.simulationId}";
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderPathForSimulation, "BudgetResultsReport.xlsx");
            if (File.Exists(filePath))
            {
                byte[] summaryReportData = File.ReadAllBytes(filePath);
                return summaryReportData;
            }
            log.Error($"BudgetResults report is not available in the path {filePath}");
            throw new FileNotFoundException($"BudgetResults report is not available in the path {filePath}", "BudgetResultsReport.xlsx");
        }
    }
}
