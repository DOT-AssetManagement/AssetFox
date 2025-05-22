using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.Common.Logging;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.Hubs;
using AppliedResearchAssociates.iAM.Hubs.Interfaces;
using AppliedResearchAssociates.iAM.Reporting.Models;
using AppliedResearchAssociates.iAM.Reporting.Services;
using AppliedResearchAssociates.iAM.Reporting.Services.UserDefinedReport;
using BridgeCareCore.Services;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System.IO;
using System.Linq;

namespace AppliedResearchAssociates.iAM.Reporting
{
    public class UserDefinedReport : IReport
    {
        private IUnitOfWork _unitOfWork;
        private IHubService _hubService;
        private readonly InitialAssetSummariesTab _initialAssetSummariesTab;
        private readonly YearTab _yearTab;
        public UserDefinedReportRequestModel _userDefinedReportRequestModel;
        private readonly ReportHelper _reportHelper;

        public UserDefinedReport(IUnitOfWork unitOfWork, string name, ReportIndexDTO results, IHubService hubService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _hubService = hubService ?? throw new ArgumentNullException(nameof(hubService));
            _reportHelper = new ReportHelper(_unitOfWork);
            ReportTypeName = name;

            _initialAssetSummariesTab = new InitialAssetSummariesTab(_unitOfWork);
            _yearTab = new YearTab(_unitOfWork);

            // check for existing report id
            var reportId = (results?.Id) ?? Guid.NewGuid();

            // set report return default parameters
            ID = (Guid)reportId;
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
            // check for the parameters string
            if (string.IsNullOrEmpty(parameters) || string.IsNullOrWhiteSpace(parameters))
            {
                Errors.Add("Parameters string is empty OR there are no parameters defined");
                IndicateError();
                return;
            }

            // Determine the Guid for the simulation and set simulation id
            var simulationId = ReportHelper.GetSimulationId(parameters);
            if (!Guid.TryParse(simulationId, out var _simulationId))
            {
                Errors.Add("Simulation ID could not be parsed to a Guid");
                IndicateError();
                return;
            }
            SimulationID = _simulationId;

            var simulationName = "";
            try
            {
                checkCancelled(cancellationToken, _simulationId);
                var simulationObject = _unitOfWork.SimulationRepo.GetSimulation(_simulationId);
                simulationName = simulationObject.Name;
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

            // Generate User defined report 
            var reportPath = "";
            try
            {
                checkCancelled(cancellationToken, _simulationId);
                // Parse parameters
                _userDefinedReportRequestModel = GetUserDefinedReportRequestModel(parameters);
                Criteria = ReportHelper.GetCriteria(parameters);
                reportPath = GenerateUserDefinedReport(_simulationId, workQueueLog, cancellationToken);
                if (!string.IsNullOrEmpty(Criteria) && string.IsNullOrEmpty(reportPath))
                {
                    var errorStatus = "No assets found for given criteria";
                    IndicateError(errorStatus);
                    Errors.Add(errorStatus);
                    return;
                }
            }
            catch (Exception e)
            {
                IndicateError();
                Errors.Add("Failed to generate user defined report");
                Errors.Add(e.Message);
                return;
            }

            if (string.IsNullOrEmpty(reportPath) || string.IsNullOrWhiteSpace(reportPath))
            {
                Errors.Add("User defined report path is missing or not set");
                IndicateError();
                return;
            }

            // Report success with location of file
            Results = reportPath;
            IsComplete = true;
            Status = "File generated.";
            return;
        }

        private string GenerateUserDefinedReport(Guid simulationId, IWorkQueueLog workQueueLog, CancellationToken? cancellationToken)
        {
            var reportDetailDto = new SimulationReportDetailDTO { SimulationId = simulationId, ReportType = ReportTypeName };

            checkCancelled(cancellationToken, simulationId);
            reportDetailDto.Status = $"Generating...";

            workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
            UpsertSimulationReportDetail(reportDetailDto);
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);

            var logger = new CallbackLogger(str => UpsertSimulationReportDetailWithStatus(reportDetailDto, str));
            var reportOutputData = _unitOfWork.SimulationOutputRepo.GetSimulationOutputViaRelation(simulationId);
            // Sort data needed? if yes should be generic numberic/text

            using var excelPackage = new ExcelPackage(new FileInfo("UserDefinedReportTestData.xlsx"));

            // InitialAssetSummariesTab based on param filters
            var filterAttributes = _userDefinedReportRequestModel.Attributes;
            // TODO remove post param Years get values from UI
            filterAttributes = reportOutputData.InitialAssetSummaries[0].ValuePerNumericAttribute.Select(_=>_.Key).ToList();
            filterAttributes.AddRange(reportOutputData.InitialAssetSummaries[0].ValuePerTextAttribute.Select(_ => _.Key).ToList());
            //
            reportDetailDto.Status = $"Creating InitialAssetSummaries tab";
            workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
            UpsertSimulationReportDetail(reportDetailDto);
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
            var assetSummariesWorksheet = excelPackage.Workbook.Worksheets.Add("InitialAssetSummaries");
            _initialAssetSummariesTab.Fill(assetSummariesWorksheet, filterAttributes, reportOutputData.InitialAssetSummaries);

            // YearTabs based on param filters
            var filterYears = _userDefinedReportRequestModel.Years;
            // TODO remove post param Years get values from UI
            filterYears = reportOutputData.Years.Select(x => x.Year).ToList();
            //
            reportDetailDto.Status = $"Creating Year tabs for selected years";                        
            workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
            UpsertSimulationReportDetail(reportDetailDto);
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
            foreach (var filterYear in filterYears)
            {
                // year tab
                var yearWorksheet = excelPackage.Workbook.Worksheets.Add("Year " + filterYear);
                var simulationYear = reportOutputData.Years.FirstOrDefault(_ => _.Year == filterYear);
                _yearTab.Fill(yearWorksheet, _userDefinedReportRequestModel, simulationYear);
                checkCancelled(cancellationToken, simulationId);
            }

            //check and generate folder
            var folderPathForSimulation = $"Reports\\{simulationId}";
            _ = Directory.CreateDirectory(folderPathForSimulation);
            var filePath = Path.Combine(folderPathForSimulation, "UserDefinedReport.xlsx");

            checkCancelled(cancellationToken, simulationId);
            var bin = excelPackage.GetAsByteArray();
            File.WriteAllBytes(filePath, bin);

            reportDetailDto.Status = $"Report Generation Completed";
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, SimulationID);
            UpsertSimulationReportDetail(reportDetailDto);

            return filePath;
        }

        private void IndicateError(string status = null)
        {
            Status = status ?? "User defined report completed with errors";
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
                Status = Status,
                ReportType = ReportTypeName
            };
            UpsertSimulationReportDetail(reportDetailDto);
        }        

        private void UpsertSimulationReportDetail(SimulationReportDetailDTO dto) => _unitOfWork.SimulationReportDetailRepo.UpsertSimulationReportDetail(dto);

        private void UpsertSimulationReportDetailWithStatus(SimulationReportDetailDTO dto, string message)
        {
            dto.Status = message;
            UpsertSimulationReportDetail(dto);
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, dto.Status, dto.SimulationId);
        }

        private static UserDefinedReportRequestModel GetUserDefinedReportRequestModel(string parameters)
        {
            var parameterObj = JObject.Parse(parameters);
            return parameterObj.SelectToken("userDefinedReportRequestModel")?.ToObject<UserDefinedReportRequestModel>();
        }
    }
}
