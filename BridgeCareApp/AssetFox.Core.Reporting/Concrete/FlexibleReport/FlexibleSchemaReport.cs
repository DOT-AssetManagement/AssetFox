using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AssetFox.Core.Common.Logging;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Reporting.Models.FlexibleReport;
using AssetFox.Core.Reporting.Services;
using AssetFox.Core.Reporting.Services.FlexibleReport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;

namespace AssetFox.Core.Reporting
{
    public class FlexibleSchemaReport : IReport
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubService _hubService;
        private readonly ReportHelper _reportHelper;

        public FlexibleSchemaReport(IUnitOfWork unitOfWork, string name, ReportIndexDTO results, IHubService hubService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _hubService = hubService ?? throw new ArgumentNullException(nameof(hubService));
            _reportHelper = new ReportHelper(_unitOfWork);
            ReportTypeName = name;

            var reportId = results?.Id ?? Guid.NewGuid();
            ID = reportId;
            Errors = new List<string>();
            Status = "Report definition created.";
            Results = string.Empty;
            IsComplete = false;
        }

        public Guid ID { get; set; }
        public Guid? SimulationID { get; set; }
        public Guid? NetworkID { get; set; }
        public string Results { get; private set; }
        public string Suffix { get; set; }
        public ReportType Type => ReportType.File;
        public string ReportTypeName { get; private set; }
        public List<string> Errors { get; private set; }
        public bool IsComplete { get; private set; }
        public string Status { get; private set; }
        public string Criteria { get; set; }

        public async Task Run(string parameters, CancellationToken? cancellationToken = null, IWorkQueueLog workQueueLog = null)
        {
            workQueueLog ??= new DoNothingWorkQueueLog();

            if (string.IsNullOrEmpty(parameters) || string.IsNullOrWhiteSpace(parameters))
            {
                Errors.Add("Parameters string is empty OR there are no parameters defined");
                IndicateError();
                return;
            }

            // Parse simulation ID
            var simulationId = ReportHelper.GetSimulationId(parameters);
            if (!Guid.TryParse(simulationId, out var simId))
            {
                Errors.Add("Simulation ID could not be parsed to a Guid");
                IndicateError();
                return;
            }
            SimulationID = simId;

            // Parse schema ID from parameters
            Guid schemaId;
            try
            {
                var paramObj = JObject.Parse(parameters);
                var schemaIdStr = paramObj.SelectToken("schemaId")?.ToString();
                if (!Guid.TryParse(schemaIdStr, out schemaId))
                {
                    Errors.Add("Schema ID could not be parsed from parameters");
                    IndicateError();
                    return;
                }
            }
            catch (Exception ex)
            {
                Errors.Add($"Failed to parse parameters: {ex.Message}");
                IndicateError();
                return;
            }

            // Get simulation info
            string simulationName;
            Guid networkId;
            try
            {
                CheckCancelled(cancellationToken, simId);
                var simulationObject = _unitOfWork.SimulationRepo.GetSimulation(simId);
                simulationName = simulationObject.Name;
                networkId = simulationObject.NetworkId;
                NetworkID = networkId;
            }
            catch (Exception e)
            {
                IndicateError();
                Errors.Add("Failed to find simulation");
                Errors.Add(e.Message);
                return;
            }

            // Get schema
            ReportSchemaDefinition schemaDefinition;
            try
            {
                var schemaDto = _unitOfWork.ReportSchemaRepo.Get(schemaId);
                if (schemaDto == null)
                {
                    Errors.Add($"Report schema not found with ID {schemaId}");
                    IndicateError();
                    return;
                }
                schemaDefinition = SchemaParser.Parse(schemaDto.SchemaJson);
            }
            catch (Exception e)
            {
                IndicateError();
                Errors.Add("Failed to load or parse report schema");
                Errors.Add(e.Message);
                return;
            }

            // Generate report
            try
            {
                CheckCancelled(cancellationToken, simId);
                var reportDetailDto = new SimulationReportDetailDTO { SimulationId = simId, ReportType = ReportTypeName };
                reportDetailDto.Status = "Generating flexible report...";
                workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
                UpsertSimulationReportDetail(reportDetailDto);
                _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, SimulationID);

                Criteria = ReportHelper.GetCriteria(parameters);
                var reportOutputData = _unitOfWork.SimulationOutputRepo.GetSimulationOutputViaRelation(simId);

                // Apply criteria filter
                if (!string.IsNullOrEmpty(Criteria))
                {
                    _reportHelper.FilterReportOutputData(reportOutputData, networkId, Criteria);
                    if (reportOutputData.InitialAssetSummaries.Count == 0)
                    {
                        reportDetailDto.Status = "Failed to generate report due to no assets found for given criteria";
                        workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
                        UpsertSimulationReportDetail(reportDetailDto);
                        IndicateError(reportDetailDto.Status);
                        return;
                    }
                }

                CheckCancelled(cancellationToken, simId);
                reportDetailDto.Status = "Building report model...";
                workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
                UpsertSimulationReportDetail(reportDetailDto);

                // Build universal report model
                var builder = new UniversalReportBuilder();
                var universalModel = builder.Build(schemaDefinition, reportOutputData, schemaId, simId, simulationName);

                CheckCancelled(cancellationToken, simId);
                reportDetailDto.Status = "Generating Excel file...";
                workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
                UpsertSimulationReportDetail(reportDetailDto);

                // Export to Excel
                var reportPath = ExportToExcel(universalModel, simId);

                Results = reportPath;
                IsComplete = true;
                Status = "File generated.";

                reportDetailDto.Status = "Report Generation Completed";
                _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, SimulationID);
                UpsertSimulationReportDetail(reportDetailDto);
            }
            catch (Exception e)
            {
                IndicateError();
                Errors.Add("Failed to generate flexible report");
                Errors.Add(e.Message);
            }
        }

        public UniversalReportModel BuildReportModel(Guid schemaId, Guid simulationId)
        {
            var schemaDto = _unitOfWork.ReportSchemaRepo.Get(schemaId);
            if (schemaDto == null)
            {
                throw new InvalidOperationException($"Report schema not found with ID {schemaId}");
            }

            var schemaDefinition = SchemaParser.Parse(schemaDto.SchemaJson);
            var simulationObject = _unitOfWork.SimulationRepo.GetSimulation(simulationId);
            var reportOutputData = _unitOfWork.SimulationOutputRepo.GetSimulationOutputViaRelation(simulationId);

            var builder = new UniversalReportBuilder();
            return builder.Build(schemaDefinition, reportOutputData, schemaId, simulationId, simulationObject.Name);
        }

        private string ExportToExcel(UniversalReportModel model, Guid simulationId)
        {
            using var excelPackage = new ExcelPackage(new FileInfo("FlexibleReport.xlsx"));

            foreach (var section in model.Sections)
            {
                if (!section.Visible) continue;

                var tabName = section.ExcelTabName ?? section.Title ?? $"Section_{section.Order}";
                // Excel worksheet names max 31 chars
                if (tabName.Length > 31) tabName = tabName.Substring(0, 31);

                // All table-producing component types (assetDetail, treatmentTimeline,
                // budgetSummary, conditionTracking, fundedTreatmentList, goalTracking)
                if (section.TableData != null)
                {
                    WriteTableToExcel(excelPackage, tabName, section.TableData);
                }
                else if (section.AggregationData != null)
                {
                    WriteAggregationToExcel(excelPackage, tabName, section.AggregationData);
                }
                else if (section.TextData != null)
                {
                    WriteTextToExcel(excelPackage, tabName, section.TextData);
                }
            }

            var folderPathForSimulation = $"Reports\\{simulationId}";
            Directory.CreateDirectory(folderPathForSimulation);
            var filePath = Path.Combine(folderPathForSimulation, "FlexibleReport.xlsx");

            var bin = excelPackage.GetAsByteArray();
            File.WriteAllBytes(filePath, bin);

            return filePath;
        }

        private static void WriteTableToExcel(ExcelPackage package, string tabName, ReportTableData tableData)
        {
            var worksheet = package.Workbook.Worksheets.Add(tabName);
            var col = 1;

            // Write headers
            foreach (var column in tableData.Columns)
            {
                worksheet.Cells[1, col].Value = column.Header;
                worksheet.Cells[1, col].Style.Font.Bold = true;
                if (column.Width > 0)
                {
                    worksheet.Column(col).Width = column.Width;
                }
                col++;
            }

            // Write rows
            var row = 2;
            foreach (var dataRow in tableData.Rows)
            {
                col = 1;
                foreach (var cellValue in dataRow)
                {
                    worksheet.Cells[row, col].Value = cellValue;
                    col++;
                }
                row++;
            }
        }

        private static void WriteAggregationToExcel(ExcelPackage package, string tabName, ReportAggregationData aggData)
        {
            var worksheet = package.Workbook.Worksheets.Add(tabName);
            var col = 1;

            foreach (var column in aggData.Columns)
            {
                worksheet.Cells[1, col].Value = column.Header;
                worksheet.Cells[1, col].Style.Font.Bold = true;
                col++;
            }

            var row = 2;
            foreach (var dataRow in aggData.Rows)
            {
                col = 1;
                foreach (var cellValue in dataRow)
                {
                    worksheet.Cells[row, col].Value = cellValue;
                    col++;
                }
                row++;
            }
        }

        private static void WriteTextToExcel(ExcelPackage package, string tabName, ReportTextData textData)
        {
            var worksheet = package.Workbook.Worksheets.Add(tabName);
            worksheet.Cells[1, 1].Value = textData.Content;

            switch (textData.Style?.ToLowerInvariant())
            {
                case "heading":
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Style.Font.Size = 18;
                    break;
                case "subheading":
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Style.Font.Size = 14;
                    break;
                case "note":
                    worksheet.Cells[1, 1].Style.Font.Italic = true;
                    worksheet.Cells[1, 1].Style.Font.Size = 10;
                    break;
                default: // body
                    worksheet.Cells[1, 1].Style.Font.Size = 11;
                    break;
            }
        }

        private void IndicateError(string status = null)
        {
            Status = status ?? "Flexible report completed with errors";
            IsComplete = true;
        }

        private void CheckCancelled(CancellationToken? cancellationToken, Guid simulationId)
        {
            if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
            {
                throw new Exception("Report was cancelled");
            }
            var reportDetailDto = new SimulationReportDetailDTO
            {
                SimulationId = simulationId,
                Status = Status,
                ReportType = ReportTypeName
            };
            UpsertSimulationReportDetail(reportDetailDto);
        }

        private void UpsertSimulationReportDetail(SimulationReportDetailDTO dto) =>
            _unitOfWork.SimulationReportDetailRepo.UpsertSimulationReportDetail(dto);
    }
}
