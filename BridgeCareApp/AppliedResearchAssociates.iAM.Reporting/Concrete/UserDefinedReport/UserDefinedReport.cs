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
        public UserDefinedReportRequestModel _userDefinedReportRequestModel;
        private readonly ReportHelper _reportHelper;
        private readonly ConditionOfNetworkTab _conditionOfNetworkTab;
        private readonly InitialAssetsTab _initialAssetsTab;
        private readonly YearAssetsTab _yearAssetsTab;        
        private readonly BudgetsTab _budgetsTab;

        public UserDefinedReport(IUnitOfWork unitOfWork, string name, ReportIndexDTO results, IHubService hubService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _hubService = hubService ?? throw new ArgumentNullException(nameof(hubService));
            _reportHelper = new ReportHelper(_unitOfWork);
            ReportTypeName = name;
                        
            _conditionOfNetworkTab = new ConditionOfNetworkTab(_unitOfWork);
            _initialAssetsTab = new InitialAssetsTab(_unitOfWork);
            _yearAssetsTab = new YearAssetsTab(_unitOfWork);
            _budgetsTab = new BudgetsTab(_unitOfWork);

            // check for existing report id
            var reportId = (results?.Id) ?? Guid.NewGuid();

            // set report return default parameters
            ID = (Guid)reportId;
            Errors = new List<string>();
            Status = "Report definition created.";
            Results = string.Empty;
            IsComplete = false;
        }

        private Guid _networkId;

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

            // Generate User defined report 
            var reportPath = "";
            try
            {
                checkCancelled(cancellationToken, _simulationId);
                // Parse parameters
                _userDefinedReportRequestModel = GetUserDefinedReportRequestModel(parameters);
                Criteria = ReportHelper.GetCriteria(parameters);
                reportPath = GenerateUserDefinedReport(_networkId, _simulationId, workQueueLog, cancellationToken);
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

        private string GenerateUserDefinedReport(Guid networkId, Guid simulationId, IWorkQueueLog workQueueLog, CancellationToken? cancellationToken)
        {
            var reportDetailDto = new SimulationReportDetailDTO { SimulationId = simulationId, ReportType = ReportTypeName };

            checkCancelled(cancellationToken, simulationId);
            reportDetailDto.Status = $"Generating...";

            workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
            UpsertSimulationReportDetail(reportDetailDto);
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);

            var logger = new CallbackLogger(str => UpsertSimulationReportDetailWithStatus(reportDetailDto, str));
            var reportOutputData = _unitOfWork.SimulationOutputRepo.GetSimulationOutputViaRelation(simulationId);            

            // reportOutputData will be having all assets data, filter it based on criteria expression
            if (!string.IsNullOrEmpty(Criteria))
            {
                var criteriaValidationResult = _reportHelper.FilterReportOutputData(reportOutputData, networkId, Criteria);

                if (!reportOutputData.InitialAssetSummaries.Any())
                {
                    reportDetailDto.Status = "Failed to generate report due to no assets found for given criteria";
                    workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
                    UpsertSimulationReportDetail(reportDetailDto);

                    return string.Empty;
                }
            }

            var primaryKeyFields = _unitOfWork.AdminSettingsRepo.GetKeyFields();
            var firstPrimaryKey = primaryKeyFields[0].ToString();
            var isPrimaryKeyNumeric = _reportHelper.IsPrimaryKeyNumberic(reportOutputData.InitialAssetSummaries[0].ValuePerTextAttribute, reportOutputData.InitialAssetSummaries[0].ValuePerNumericAttribute, firstPrimaryKey);                        
            
            var filterYears = _userDefinedReportRequestModel.Years;
            _ = reportOutputData.Years.RemoveAll(_ => !filterYears.Contains(_.Year));

            // Sort data
            if (isPrimaryKeyNumeric)
            {
                reportOutputData.InitialAssetSummaries.Sort(
                    (a, b) => _reportHelper.CheckAndGetValue<double>(a.ValuePerNumericAttribute, firstPrimaryKey).CompareTo(_reportHelper.CheckAndGetValue<double>(b.ValuePerNumericAttribute, firstPrimaryKey))
                );

                foreach (var yearlySectionData in reportOutputData.Years)
                {
                    yearlySectionData.Assets.Sort(
                        (a, b) => _reportHelper.CheckAndGetValue<double>(a.ValuePerNumericAttribute, firstPrimaryKey).CompareTo(_reportHelper.CheckAndGetValue<double>(b.ValuePerNumericAttribute, firstPrimaryKey))
                        );
                }
            }
            else
            {
                reportOutputData.InitialAssetSummaries.Sort(
                    (a, b) => _reportHelper.CheckAndGetValue<string>(a.ValuePerTextAttribute, firstPrimaryKey).CompareTo(_reportHelper.CheckAndGetValue<string>(b.ValuePerTextAttribute, firstPrimaryKey))
                );

                foreach (var yearlySectionData in reportOutputData.Years)
                {
                    yearlySectionData.Assets.Sort(
                        (a, b) => _reportHelper.CheckAndGetValue<string>(a.ValuePerTextAttribute, firstPrimaryKey).CompareTo(_reportHelper.CheckAndGetValue<string>(b.ValuePerTextAttribute, firstPrimaryKey))
                        );
                }
            }

            using var excelPackage = new ExcelPackage(new FileInfo("UserDefinedReportTestData.xlsx"));
            
            if (_userDefinedReportRequestModel.DisplayConditionOfNetwork)
            {
                reportDetailDto.Status = $"Creating Condition Of Network tab";
                workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
                UpsertSimulationReportDetail(reportDetailDto);
                _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
                var conditionOfNetwokWorksheet = excelPackage.Workbook.Worksheets.Add("Condition Of Network");
                _conditionOfNetworkTab.Fill(conditionOfNetwokWorksheet, reportOutputData.Years);
                checkCancelled(cancellationToken, simulationId);
            }

            var filterAttributes = _userDefinedReportRequestModel.Attributes;
            // Initial Assets - use filterAttributes
            if (_userDefinedReportRequestModel.DisplayInitialAssets)
            {
                reportDetailDto.Status = $"Creating Initial Assets tab";
                workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
                UpsertSimulationReportDetail(reportDetailDto);
                _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
                var assetSummariesWorksheet = excelPackage.Workbook.Worksheets.Add("Initial Assets");
                _initialAssetsTab.Fill(assetSummariesWorksheet, filterAttributes, isPrimaryKeyNumeric, firstPrimaryKey, reportOutputData.InitialAssetSummaries);
                checkCancelled(cancellationToken, simulationId);
            }

            // Year Assets - use filterAttributes and filterYears
            if (_userDefinedReportRequestModel.DisplayYearAssets)
            {
                reportDetailDto.Status = $"Creating Year Assets tab";
                workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
                UpsertSimulationReportDetail(reportDetailDto);
                _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
                var yearAssetsWorksheet = excelPackage.Workbook.Worksheets.Add("Year Assets");
                _yearAssetsTab.Fill(yearAssetsWorksheet, filterAttributes, isPrimaryKeyNumeric, firstPrimaryKey, reportOutputData.InitialAssetSummaries, reportOutputData.Years);
                checkCancelled(cancellationToken, simulationId);
            }
                        
            if(_userDefinedReportRequestModel.DisplayBudgets)
            {
                reportDetailDto.Status = $"Creating Budgets tab";
                workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
                UpsertSimulationReportDetail(reportDetailDto);
                _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
                var budgetsWorksheet = excelPackage.Workbook.Worksheets.Add("Budgets");
                _budgetsTab.Fill(budgetsWorksheet,  isPrimaryKeyNumeric, firstPrimaryKey, reportOutputData.InitialAssetSummaries, reportOutputData.Years);
                checkCancelled(cancellationToken, simulationId);
            }

            if (_userDefinedReportRequestModel.DisplayDeficientConditionGoals)
            {

            }

            if (_userDefinedReportRequestModel.DisplayTargetConditionGoals)
            {

            }

            if (_userDefinedReportRequestModel.DisplayTreatmentOptions)
            {

            }

            if (_userDefinedReportRequestModel.DisplayTreatmentSchedulingCollisions)
            {

            }

            if (_userDefinedReportRequestModel.DisplayTreatmentRejections)
            {

            }

            if (_userDefinedReportRequestModel.DisplayTreatmentCashflowConsiderations)
            {

            }

            if (_userDefinedReportRequestModel.DisplyTreatmentCurrentBudgetsToSpend)
            {

            }

            if (_userDefinedReportRequestModel.DisplayTreatmentAllocations)
            {

            }

            // check and generate folder
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
