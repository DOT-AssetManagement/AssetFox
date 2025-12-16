using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssetFox.Core.Common.Logging;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Reporting.Services;
using AssetFox.Core.Reporting.Services.PAMSPBExport;
using AssetFoxCore.Services;
using OfficeOpenXml;

namespace AssetFox.Core.Reporting
{
    public class PAMSPBExportReport : IReport
    {
        protected readonly IHubService _hubService;
        private readonly IUnitOfWork _unitOfWork;
        private Guid _networkId;
        private readonly TreatmentTab _treatmentTab;
        private readonly MASTab _masTab;

        public PAMSPBExportReport(IUnitOfWork unitOfWork, string name, ReportIndexDTO results, IHubService hubService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _hubService = hubService ?? throw new ArgumentNullException(nameof(hubService));
            ReportTypeName = name;
            _treatmentTab = new TreatmentTab(_unitOfWork);
            _masTab = new MASTab();

            // Check for existing report id
            var reportId = results?.Id; if (reportId == null) { reportId = Guid.NewGuid(); }

            // Set report return default parameters
            ID = (Guid)reportId;
            Errors = new List<string>();
            Warnings = new List<string>();
            Status = "Report definition created.";
            Results = string.Empty;
            IsComplete = false;
        }

        public Guid ID { get; set; }

        public Guid? SimulationID { get; set; }

        public Guid? NetworkID { get; set; }

        public string Results { get; private set; }

        public ReportType Type => ReportType.File;

        public string ReportTypeName { get; private set; }

        public List<string> Errors { get; private set; }

        public List<string> Warnings { get; set; }

        public bool IsComplete { get; private set; }

        public string Status { get; private set; }

        public string Suffix => throw new NotImplementedException();
        
        public string Criteria { get; set; }

        public async Task Run(string parameters, CancellationToken? cancellationToken = null, IWorkQueueLog workQueueLog = null)
        {
            workQueueLog ??= new DoNothingWorkQueueLog();
            // Check for the parameters
            if (string.IsNullOrEmpty(parameters) || string.IsNullOrWhiteSpace(parameters))
            {
                Errors.Add("No simulation ID provided in the parameters of PAMS Simulation Report runner");
                IndicateError();
                return;
            }

            // Set simulation id
            string simulationId = ReportHelper.GetSimulationId(parameters);
            if (!Guid.TryParse(simulationId, out var _simulationId))
            {
                Errors.Add("Provided simulation ID is not a GUID");
                IndicateError();
                return;
            }
            SimulationID = _simulationId;

            var simulationName = string.Empty;
            try
            {
                var simulationObject = _unitOfWork.SimulationRepo.GetSimulation(_simulationId);
                simulationName = simulationObject.Name;
                _networkId = simulationObject.NetworkId;
            }
            catch (Exception e)
            {
                IndicateError();
                Errors.Add("Failed to find simulation");
                Errors.Add(e.Message);
                return;
            }

            // Check for simulation existence                        
            if (simulationName == null)
            {
                IndicateError();
                Errors.Add($"Failed to find name using simulation ID {_simulationId}.");
                return;
            }

            // Generate report 
            var reportPath = string.Empty;
            try
            {
                reportPath = GeneratePAMSPBExportReport(_networkId, _simulationId, workQueueLog, cancellationToken);
            }
            catch (Exception e)
            {
                IndicateError();
                Errors.Add("Failed to generate PAMS PB Export report");
                Errors.Add(e.Message);
                return;
            }

            if (string.IsNullOrEmpty(reportPath) || string.IsNullOrWhiteSpace(reportPath))
            {
                Errors.Add("PAMS PB Export report path is missing or not set");
                IndicateError();
                return;
            }

            // Report success with location of file
            Results = reportPath;
            IsComplete = true;
            Status = "File generated.";
            return;
        }

        private string GeneratePAMSPBExportReport(Guid networkId, Guid simulationId, IWorkQueueLog workQueueLog, CancellationToken? cancellationToken = null)
        {
            checkCancelled(cancellationToken, simulationId);
            var reportPath = string.Empty;
            var reportDetailDto = new SimulationReportDetailDTO
            {
                SimulationId = simulationId,
                Status = $"Generating...",
                ReportType = ReportTypeName
            };
            workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
            UpsertSimulationReportDetail(reportDetailDto);
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);

            var logger = new CallbackLogger(str => UpsertSimulationReportDetailWithStatus(reportDetailDto, str));

            var networkMaintainableAssets = _unitOfWork.MaintainableAssetRepo.GetAllInNetworkWithLocations(networkId);
            var networkMaintainableAssetIds = networkMaintainableAssets.Select(x => x.Id);
            var attributeDtos = _unitOfWork.AttributeRepo.GetAttributes();
            var requiredAttributeIds = GetRequiredAttributeIds(attributeDtos);
            var aggregatedResultDtos = _unitOfWork.AggregatedResultRepo.GetAllInNetwork(networkId);
            var simulationDto = _unitOfWork.SimulationRepo.GetSimulation(simulationId);
            var analysisMethodDto = _unitOfWork.AnalysisMethodRepo.GetAnalysisMethod(simulationId);
            var scenarioSelectableTreatmentsDtos = _unitOfWork.SelectableTreatmentRepo.GetScenarioSelectableTreatmentsForReport(simulationId);
            var committedProjectList = _unitOfWork.CommittedProjectRepo.GetCommittedProjectsForExport(simulationId);
            var primaryKeyFields = _unitOfWork.AdminSettingsRepo.GetKeyFields();
            var firstPrimaryKey = primaryKeyFields[0].ToString();

            // Report
            using var excelPackage = new ExcelPackage(new FileInfo("PAMSPBExportReportData.xlsx"));

            // MAS Tab
            reportDetailDto.Status = $"Creating PAMS MAS TAB";
            UpsertSimulationReportDetail(reportDetailDto);
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
            var masWorksheet = excelPackage.Workbook.Worksheets.Add(PAMSPBExportReportConstants.MASTab);
            _masTab.Fill(masWorksheet, networkId, networkMaintainableAssets, aggregatedResultDtos, attributeDtos);

            // Teatments Tab
            reportDetailDto.Status = $"Creating PAMS Treatments TAB";
            UpsertSimulationReportDetail(reportDetailDto);
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
            var simulationOutput = _unitOfWork.SimulationOutputRepo.GetSimulationOutputViaRelation(simulationId, attributeDtos: attributeDtos);
            var treatmentsWorksheet = excelPackage.Workbook.Worksheets.Add(PAMSPBExportReportConstants.TreatmentTab);
            var shouldBundleFeasibleTreatments = analysisMethodDto.ShouldAllowMultipleTreatments;
            _treatmentTab.Fill(treatmentsWorksheet, simulationOutput, simulationId, networkId, scenarioSelectableTreatmentsDtos, networkMaintainableAssets, shouldBundleFeasibleTreatments, committedProjectList, firstPrimaryKey);

            checkCancelled(cancellationToken, simulationId);            

            // Check and generate folder
            reportDetailDto.Status = $"Creating Report file";
            workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
            UpsertSimulationReportDetail(reportDetailDto);
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
            var folderPathForSimulation = $"Reports\\{simulationId}";
            _ = Directory.CreateDirectory(folderPathForSimulation);
            reportPath = Path.Combine(folderPathForSimulation, "PAMSPBExportReport.xlsx");
            checkCancelled(cancellationToken, simulationId);
            var bin = excelPackage.GetAsByteArray();
            File.WriteAllBytes(reportPath, bin);

            reportDetailDto.Status = $"Report generation completed";
            UpsertSimulationReportDetail(reportDetailDto);
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);

            return reportPath;
        }

        private static List<Guid> GetRequiredAttributeIds(List<AttributeDTO> attributeDTOs)
        {
            var requiredAttributes = new List<string>
            {
                "SEGMENT_LENGTH","WIDTH","DISTRICT","CNTY","SR","DIRECTION","INTERSTATE","LANES","SURFACE_NAME","RISKSCORE"
            };
            return attributeDTOs.Where(_ => requiredAttributes.Contains(_.Name)).Select(_ => _.Id).ToList();
        }

        private void UpsertSimulationReportDetail(SimulationReportDetailDTO dto) => _unitOfWork.SimulationReportDetailRepo.UpsertSimulationReportDetail(dto);

        private void UpsertSimulationReportDetailWithStatus(SimulationReportDetailDTO dto, string message)
        {
            dto.Status = message;
            UpsertSimulationReportDetail(dto);
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, dto.Status, dto.SimulationId);
        }

        private void IndicateError()
        {
            Status = "PAMS PB Export output report completed with errors";
            IsComplete = true;
        }

        private void checkCancelled(CancellationToken? cancellationToken, Guid simulationId)
        {
            if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
            {
                throw new Exception("Report was cancelled");
            }
            var reportDetailDto = new SimulationReportDetailDTO
            {
                SimulationId = simulationId,
                Status = $"",
                ReportType = ReportTypeName
            };
            UpsertSimulationReportDetail(reportDetailDto);
        }
    }
}
