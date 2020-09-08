using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using BridgeCare.Interfaces;
using BridgeCare.Interfaces.ConditionResults;
using BridgeCare.Interfaces.ReportsDownload;
using BridgeCare.Models;
using BridgeCare.Properties;
using BridgeCare.Services.CommonData;
using Hangfire;
using MongoDB.Driver;
using OfficeOpenXml;

namespace BridgeCare.Services.ConditionResultReport
{
    public class ConditionResultReportGenerator : IConditionResultReportGenerator, IReportsDownload<ConditionResultReportGenerator>
    {
        private readonly ICommonSummaryReportData commonSummaryReportData;
        private readonly CommonBridgeData commonBridgeData;
        private readonly ConditionResultDataTAB conditionResultDataTAB;
        private readonly ConditionDistributionGraphTAB conditionDistributionGraphTAB;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ConditionResultReportGenerator));

        public ConditionResultReportGenerator(ICommonSummaryReportData commonSummaryReportData, CommonBridgeData commonBridgeData,
            ConditionResultDataTAB conditionResultDataTAB,
            ConditionDistributionGraphTAB conditionDistributionGraphTAB)
        {
            this.commonSummaryReportData = commonSummaryReportData ??
                throw new ArgumentNullException(nameof(commonSummaryReportData));
            this.commonBridgeData = commonBridgeData ??
                throw new ArgumentNullException(nameof(commonBridgeData));
            this.conditionResultDataTAB = conditionResultDataTAB ?? throw new ArgumentNullException(nameof(conditionResultDataTAB));
            this.conditionDistributionGraphTAB = conditionDistributionGraphTAB ?? throw new ArgumentNullException(nameof(conditionDistributionGraphTAB));
        }

        [AutomaticRetry(Attempts = 0)]
        public void GenerateConditionResultReport(SimulationModel simulationModel)
        {
            // Get data
            var simulationId = simulationModel.simulationId;
            var simulationYearsModel = commonSummaryReportData.GetSimulationYearsData(simulationId);
            var simulationYears = simulationYearsModel.Years;
            simulationYears.Sort();
            var simulationYearsCount = simulationYears.Count;
            var dbContext = new BridgeCareContext();

            using (var excelPackage = new ExcelPackage(new System.IO.FileInfo("ConditionResultReport.xlsx")))
            {
#if DEBUG
                var mongoConnection = ConfigurationManager.ConnectionStrings["MongoDBDevConnectionString"].ConnectionString;
#else
                var mongoConnection = ConfigurationManager.ConnectionStrings["MongoDBProdConnectionString"].ConnectionString;
#endif
                var client = new MongoClient(mongoConnection);
                var MongoDatabase = client.GetDatabase("BridgeCare");
                var simulations = MongoDatabase.GetCollection<SimulationModel>("scenarios");

                var updateStatus = Builders<SimulationModel>.Update
                    .Set(s => s.status, "Begin condition result report generation");
                simulations.UpdateOne(s => s.simulationId == simulationId, updateStatus);

                var commonBridgeDataModel = commonBridgeData.Get(simulationModel, simulationYears, dbContext);

                var worksheet = excelPackage.Workbook.Worksheets.Add("Bridge Data");
                worksheet.Hidden = eWorkSheetHidden.VeryHidden;

                var conditionResultWorkSheet = excelPackage.Workbook.Worksheets.Add("Data for condition result");
                var chartRowsModel = conditionResultDataTAB.Fill(conditionResultWorkSheet, commonBridgeDataModel.SimulationDataModels, simulationYears,
                    dbContext, simulationId);

                // Condition DA tab
                worksheet = excelPackage.Workbook.Worksheets.Add("Conditions");
                conditionDistributionGraphTAB.Fill(worksheet, conditionResultWorkSheet, chartRowsModel.TotalDeckAreaPercentYearsRow,
                    chartRowsModel.TotalBridgeCareBudgetPerYear,
                    simulationYearsCount);

                var folderPathForSimulation = $"DownloadedReports\\ConditionResult\\{simulationModel.simulationId}";
                string relativeFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderPathForSimulation);
                Directory.CreateDirectory(relativeFolderPath);
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderPathForSimulation, "ConditionResultReport.xlsx");
                byte[] bin = excelPackage.GetAsByteArray();
                File.WriteAllBytes(filePath, bin);

                updateStatus = Builders<SimulationModel>.Update
                    .Set(s => s.status, "Condition result report has been generated");
                simulations.UpdateOne(s => s.simulationId == simulationId, updateStatus);
            }
        }

        public byte[] DownloadExcelReport(SimulationModel simulationModel)
        {
            var folderPathForSimulation = $"DownloadedReports\\ConditionResult\\{simulationModel.simulationId}";
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderPathForSimulation, "ConditionResultReport.xlsx");
            if (File.Exists(filePath))
            {
                byte[] summaryReportData = File.ReadAllBytes(filePath);
                return summaryReportData;
            }
            log.Error($"Condition result report is not available in the path {filePath}");
            throw new FileNotFoundException($"Condition result report is not available in the path {filePath}", "ConditionResultReport.xlsx");
        }
    }
}
