using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AssetFox.Core.Common.Logging;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Reporting.Models;
using AssetFox.Core.Reporting.Services;
using AssetFox.Core.Reporting.Services.UserDefinedReport;
using AssetFoxCore.Services;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System.IO;

namespace AssetFox.Core.Reporting
{
    public class UserDefinedReport : IReport
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubService _hubService;        
        public UserDefinedReportRequestModel _userDefinedReportRequestModel;
        private readonly ReportHelper _reportHelper;
        private readonly InitialAssetsTab _initialAssetsTab;
        private readonly YearAssetsTab _yearAssetsTab;
        private readonly TreatmentOptionsTab _treatmentOptionsTab;
        private readonly TreatmentSchedulingCollisionsTab _treatmentSchedulingCollisionsTab;
        private readonly TreatmentRejectionsTab _treatmentRejectionsTab;
        private readonly TreatmentCashflowConsiderationsTab _treatmentCashflowConsiderationsTab;
        private readonly TreatmentCurrentBudgetsToSpendTab _treatmentCurrentBudgetsToSpendTab;
        private readonly TreatmentAllocationsTab _treatmentAllocationsTab;

        public UserDefinedReport(IUnitOfWork unitOfWork, string name, ReportIndexDTO results, IHubService hubService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _hubService = hubService ?? throw new ArgumentNullException(nameof(hubService));
            _reportHelper = new ReportHelper(_unitOfWork);
            ReportTypeName = name;
                        
            _initialAssetsTab = new InitialAssetsTab(_unitOfWork);
            _yearAssetsTab = new YearAssetsTab(_unitOfWork);
            _treatmentOptionsTab = new TreatmentOptionsTab(_unitOfWork);
            _treatmentSchedulingCollisionsTab = new TreatmentSchedulingCollisionsTab(_unitOfWork);
            _treatmentRejectionsTab = new TreatmentRejectionsTab(_unitOfWork);
            _treatmentCashflowConsiderationsTab = new TreatmentCashflowConsiderationsTab(_unitOfWork);
            _treatmentCurrentBudgetsToSpendTab = new TreatmentCurrentBudgetsToSpendTab(_unitOfWork);
            _treatmentAllocationsTab = new TreatmentAllocationsTab(_unitOfWork);

            // check for existing report id
            var reportId = (results?.Id) ?? Guid.NewGuid();

            // set report return default parameters
            ID = (Guid)reportId;
            Errors = [];
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
            updateStatusSendMessage();

            var logger = new CallbackLogger(str => UpsertSimulationReportDetailWithStatus(reportDetailDto, str));
            var reportOutputData = _unitOfWork.SimulationOutputRepo.GetSimulationOutputViaRelation(simulationId);            

            // reportOutputData will be having all assets data, filter it based on criteria expression
            if (!string.IsNullOrEmpty(Criteria))
            {
                var criteriaValidationResult = _reportHelper.FilterReportOutputData(reportOutputData, networkId, Criteria);

                if (reportOutputData.InitialAssetSummaries.Count == 0)
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
                updateStatusSendMessage();
                var conditionOfNetwokWorksheet = excelPackage.Workbook.Worksheets.Add("Condition Of Network");
                ConditionOfNetworkTab.Fill(conditionOfNetwokWorksheet, reportOutputData.Years);
                checkCancelled(cancellationToken, simulationId);
            }

            var filterAttributes = _userDefinedReportRequestModel.Attributes;
            // Initial Assets - use filterAttributes
            if (_userDefinedReportRequestModel.DisplayInitialAssets)
            {
                reportDetailDto.Status = $"Creating Initial Assets tab";
                updateStatusSendMessage();
                var assetSummariesWorksheet = excelPackage.Workbook.Worksheets.Add("Initial Assets");
                _initialAssetsTab.Fill(assetSummariesWorksheet, filterAttributes, isPrimaryKeyNumeric, firstPrimaryKey, reportOutputData.InitialAssetSummaries);
                checkCancelled(cancellationToken, simulationId);
            }

            // Year Assets - use filterAttributes and filterYears
            if (_userDefinedReportRequestModel.DisplayYearAssets)
            {
                reportDetailDto.Status = $"Creating Year Assets tab";
                updateStatusSendMessage();
                var yearAssetsWorksheet = excelPackage.Workbook.Worksheets.Add("Year Assets");
                _yearAssetsTab.Fill(yearAssetsWorksheet, filterAttributes, isPrimaryKeyNumeric, firstPrimaryKey, reportOutputData.InitialAssetSummaries, reportOutputData.Years);
                checkCancelled(cancellationToken, simulationId);
            }
                        
            if(_userDefinedReportRequestModel.DisplayBudgets)
            {
                reportDetailDto.Status = $"Creating Budgets tab";
                updateStatusSendMessage();
                var budgetsWorksheet = excelPackage.Workbook.Worksheets.Add("Budgets");
                BudgetsTab.Fill(budgetsWorksheet, reportOutputData.Years);
                checkCancelled(cancellationToken, simulationId);
            }

            if (_userDefinedReportRequestModel.DisplayDeficientConditionGoals)
            {
                reportDetailDto.Status = $"Creating DeficientConditionGoals tab";
                updateStatusSendMessage();
                var deficientConditionGoalsWorksheet = excelPackage.Workbook.Worksheets.Add("DeficientConditionGoals");
                DeficientConditionGoalsTab.Fill(deficientConditionGoalsWorksheet, reportOutputData.Years);
                checkCancelled(cancellationToken, simulationId);
            }

            if (_userDefinedReportRequestModel.DisplayTargetConditionGoals)
            {
                reportDetailDto.Status = $"Creating TargetConditionGoals tab";
                updateStatusSendMessage();
                var targetConditionGoalsWorksheet = excelPackage.Workbook.Worksheets.Add("TargetConditionGoals");
                TargetConditionGoalsTab.Fill(targetConditionGoalsWorksheet, reportOutputData.Years);
                checkCancelled(cancellationToken, simulationId);
            }

            if (_userDefinedReportRequestModel.DisplayTreatmentOptions)
            {
                reportDetailDto.Status = $"Creating TreatmentOptions tab";
                updateStatusSendMessage();
                var treatmentOptionsWorksheet = excelPackage.Workbook.Worksheets.Add("TreatmentOptions");
                _treatmentOptionsTab.Fill(treatmentOptionsWorksheet, isPrimaryKeyNumeric, firstPrimaryKey, reportOutputData.InitialAssetSummaries, reportOutputData.Years);
                checkCancelled(cancellationToken, simulationId);
            }

            if (_userDefinedReportRequestModel.DisplayTreatmentSchedulingCollisions)
            {
                reportDetailDto.Status = $"Creating TreatmentSchedulingCollisions tab";
                updateStatusSendMessage();
                var treatmentSchedulingCollisionsWorksheet = excelPackage.Workbook.Worksheets.Add("TreatmentSchedulingCollisions");
                _treatmentSchedulingCollisionsTab.Fill(treatmentSchedulingCollisionsWorksheet, isPrimaryKeyNumeric, firstPrimaryKey, reportOutputData.InitialAssetSummaries, reportOutputData.Years);
                checkCancelled(cancellationToken, simulationId);
            }

            if (_userDefinedReportRequestModel.DisplayTreatmentRejections)
            {
                reportDetailDto.Status = $"Creating TreatmentRejections tab";
                updateStatusSendMessage();
                var treatmentRejectionsWorksheet = excelPackage.Workbook.Worksheets.Add("TreatmentRejections");
                _treatmentRejectionsTab.Fill(treatmentRejectionsWorksheet, isPrimaryKeyNumeric, firstPrimaryKey, reportOutputData.InitialAssetSummaries, reportOutputData.Years);
                checkCancelled(cancellationToken, simulationId);
            }

            if (_userDefinedReportRequestModel.DisplayTreatmentCashflowConsiderations)
            {
                reportDetailDto.Status = $"Creating TreatmentCashflowConsiderations tab";
                updateStatusSendMessage();
                var treatmentCashflowConsiderationsWorksheet = excelPackage.Workbook.Worksheets.Add("TreatmentCashflowConsiderations");
                _treatmentCashflowConsiderationsTab.Fill(treatmentCashflowConsiderationsWorksheet, isPrimaryKeyNumeric, firstPrimaryKey, reportOutputData.InitialAssetSummaries, reportOutputData.Years);
                checkCancelled(cancellationToken, simulationId);
            }

            if (_userDefinedReportRequestModel.DisplyTreatmentCurrentBudgetsToSpend)
            {
                reportDetailDto.Status = $"Creating TreatmentCurrentBudgetsToSpend tab";
                updateStatusSendMessage();
                var treatmentCurrentBudgetsToSpendWorksheet = excelPackage.Workbook.Worksheets.Add("TreatmentCurrentBudgetsToSpend");
                _treatmentCurrentBudgetsToSpendTab.Fill(treatmentCurrentBudgetsToSpendWorksheet, isPrimaryKeyNumeric, firstPrimaryKey, reportOutputData.InitialAssetSummaries, reportOutputData.Years);
                checkCancelled(cancellationToken, simulationId);
            }

            if (_userDefinedReportRequestModel.DisplayTreatmentAllocations)
            {
                reportDetailDto.Status = $"Creating TreatmentAllocations tab";
                updateStatusSendMessage();
                var treatmentAllocationsWorksheet = excelPackage.Workbook.Worksheets.Add("TreatmentAllocations");
                _treatmentAllocationsTab.Fill(treatmentAllocationsWorksheet, isPrimaryKeyNumeric, firstPrimaryKey, reportOutputData.InitialAssetSummaries, reportOutputData.Years);
                checkCancelled(cancellationToken, simulationId);
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

            void updateStatusSendMessage()
            {
                workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
                UpsertSimulationReportDetail(reportDetailDto);
                _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
            }
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
