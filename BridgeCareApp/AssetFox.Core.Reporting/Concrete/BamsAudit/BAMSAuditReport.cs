using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.Common.Logging;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Reporting.Services;
using AssetFox.Core.Reporting.Services.BAMSAuditReport;
using AssetFoxCore.Services;
using OfficeOpenXml;

namespace AssetFox.Core.Reporting
{
    public class BAMSAuditReport : IReport
    {
        protected readonly IHubService _hubService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DataTab _dataTab;
        private readonly DecisionTab _decisionTab;
        private readonly ReportHelper _reportHelper;

        public BAMSAuditReport(IUnitOfWork unitOfWork, string name, ReportIndexDTO results, IHubService hubService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _hubService = hubService ?? throw new ArgumentNullException(nameof(hubService));
            ReportTypeName = name;            
            _dataTab = new DataTab(_unitOfWork);
            _decisionTab = new DecisionTab(_unitOfWork);
            _reportHelper = new ReportHelper(_unitOfWork);

            // check for existing report id
            var reportId = results?.Id; if (reportId == null) { reportId = Guid.NewGuid(); }

            // set report return default parameters
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
                Errors.Add("Parameters string is empty OR there are no parameters defined");
                IndicateError();
                return;
            }

            // Set simulation id
            string simulationId = ReportHelper.GetSimulationId(parameters);
            if (!Guid.TryParse(simulationId, out Guid _simulationId))
            {
                Errors.Add("Simulation ID could not be parsed to a Guid");
                IndicateError();
                return;
            }
            SimulationID = _simulationId;

            var simulationName = string.Empty;
            try
            {
                var simulationObject = _unitOfWork.SimulationRepo.GetSimulation(_simulationId);
                simulationName = simulationObject.Name;
                var _networkId = simulationObject.NetworkId;
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
                reportPath = GenerateBAMSAuditReport(_simulationId, ReportTypeName, workQueueLog, cancellationToken);
            }
            catch (Exception e)
            {
                IndicateError();
                Errors.Add("Failed to generate Audit report");
                Errors.Add(e.Message);
                return;
            }

            if (string.IsNullOrEmpty(reportPath) || string.IsNullOrWhiteSpace(reportPath))
            {
                Errors.Add("Audit report path is missing or not set");
                IndicateError();
                return;
            }

            // Report success with location of file
            Results = reportPath;
            IsComplete = true;
            Status = "File generated.";
            return;
        }

        private string GenerateBAMSAuditReport(Guid simulationId, string ReportTypeName, IWorkQueueLog workQueueLog, CancellationToken? cancellationToken = null)
        {
            if(cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
            {
                throw new Exception("Report was cancelled");
            }
            var reportPath = string.Empty;
            var reportDetailDto = new SimulationReportDetailDTO
            {
                SimulationId = simulationId,
                Status = $"Generating...",
                ReportType = ReportTypeName
            };
            UpsertSimulationReportDetail(reportDetailDto);
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
            workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);

            var logger = new CallbackLogger(str => UpsertSimulationReportDetailWithStatus(reportDetailDto, str));

            var simulationOutput = _unitOfWork.SimulationOutputRepo.GetSimulationOutputViaRelation(simulationId);
            var primaryKeyFields = _unitOfWork.AdminSettingsRepo.GetKeyFields();
            var firstPrimaryKey = primaryKeyFields[0].ToString();
            var isPrimaryKeyNumeric = _reportHelper.IsPrimaryKeyNumberic(simulationOutput.InitialAssetSummaries[0].ValuePerTextAttribute, simulationOutput.InitialAssetSummaries[0].ValuePerNumericAttribute, firstPrimaryKey);

            // Sort data
            if (isPrimaryKeyNumeric)
            {
                simulationOutput.InitialAssetSummaries.Sort(
                        (a, b) => _reportHelper.CheckAndGetValue<double>(a.ValuePerNumericAttribute, firstPrimaryKey).CompareTo(_reportHelper.CheckAndGetValue<double>(b.ValuePerNumericAttribute, firstPrimaryKey))
                        );

                foreach (var yearlySectionData in simulationOutput.Years)
                {
                    yearlySectionData.Assets.Sort(
                        (a, b) => _reportHelper.CheckAndGetValue<double>(a.ValuePerNumericAttribute, firstPrimaryKey).CompareTo(_reportHelper.CheckAndGetValue<double>(b.ValuePerNumericAttribute, firstPrimaryKey))
                        );
                }
            }
            else
            {
                simulationOutput.InitialAssetSummaries.Sort(
                        (a, b) => _reportHelper.CheckAndGetValue<string>(a.ValuePerTextAttribute, firstPrimaryKey).CompareTo(_reportHelper.CheckAndGetValue<string>(b.ValuePerTextAttribute, firstPrimaryKey))
                        );

                foreach (var yearlySectionData in simulationOutput.Years)
                {
                    yearlySectionData.Assets.Sort(
                        (a, b) => _reportHelper.CheckAndGetValue<string>(a.ValuePerTextAttribute, firstPrimaryKey).CompareTo(_reportHelper.CheckAndGetValue<string>(b.ValuePerTextAttribute, firstPrimaryKey))
                        );
                }
            }

            var analysisMethodDto = _unitOfWork.AnalysisMethodRepo.GetAnalysisMethod(simulationId);
            var performanceCurvesDtos = _unitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            var scenarioSelectableTreatmentsDtos = _unitOfWork.SelectableTreatmentRepo.GetScenarioSelectableTreatmentsForReport(simulationId);            

            // Report
            using var excelPackage = new ExcelPackage(new FileInfo("BAMSAuditReportData.xlsx"));

            checkCancelled(cancellationToken, simulationId);
            // Bridge Data TAB
            reportDetailDto.Status = $"Creating Data TAB";
            UpsertSimulationReportDetail(reportDetailDto);
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
            workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
            var bridgesWorksheet = excelPackage.Workbook.Worksheets.Add(BAMSAuditReportConstants.BridgesTab);
            var dataTabRequiredAttributes = DataTab.GetRequiredAttributes();
            ValidateSections(simulationOutput, reportDetailDto, simulationId, dataTabRequiredAttributes);
            _dataTab.Fill(bridgesWorksheet, simulationOutput, firstPrimaryKey);

            checkCancelled(cancellationToken, simulationId);
            // Fill Decisions TAB
            reportDetailDto.Status = $"Creating Decision TAB";
            UpsertSimulationReportDetail(reportDetailDto);
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
            workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
            var decisionsWorksheet = excelPackage.Workbook.Worksheets.Add(BAMSAuditReportConstants.DecisionsTab);
            var performanceCurvesAttributes = _reportHelper.GetPerformanceCurvesAttributes(performanceCurvesDtos);
            performanceCurvesDtos.Clear();
            ValidateSections(simulationOutput, reportDetailDto, simulationId, new HashSet<string>(performanceCurvesAttributes.Except(dataTabRequiredAttributes)));
            _decisionTab.Fill(decisionsWorksheet, simulationOutput, performanceCurvesAttributes, analysisMethodDto, scenarioSelectableTreatmentsDtos, firstPrimaryKey);

            checkCancelled(cancellationToken, simulationId);
            // Check and generate folder
            reportDetailDto.Status = $"Creating Report file";
            UpsertSimulationReportDetail(reportDetailDto);
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
            workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);
            var folderPathForSimulation = $"Reports\\{simulationId}";
            _ = Directory.CreateDirectory(folderPathForSimulation);
            reportPath = Path.Combine(folderPathForSimulation, "BAMSAuditReport.xlsx");

            var bin = excelPackage.GetAsByteArray();
            File.WriteAllBytes(reportPath, bin);                       
            
            checkCancelled(cancellationToken, simulationId);
            reportDetailDto.Status = $"Report generation completed";          
            UpsertSimulationReportDetail(reportDetailDto);
            _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
            workQueueLog.UpdateWorkQueueStatus(reportDetailDto.Status);

            return reportPath;
        }

        private void ValidateSections(SimulationOutput simulationOutput,SimulationReportDetailDTO reportDetailDto, Guid simulationId, HashSet<string> requiredAttributes)
        {
            var initialSectionValues = simulationOutput.InitialAssetSummaries[0].ValuePerNumericAttribute;
            var sectionValueAttribute = simulationOutput.Years[0].Assets[0].ValuePerNumericAttribute;
            foreach (var item in requiredAttributes)
            {
                if (!initialSectionValues.ContainsKey(item))
                {
                    reportDetailDto.Status = $"{item} was not found in initial section";
                    UpsertSimulationReportDetail(reportDetailDto);
                    _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
                    Errors.Add(reportDetailDto.Status);
                    throw new KeyNotFoundException($"{item} was not found in initial section");
                }

                if (!sectionValueAttribute.ContainsKey(item))
                {
                    reportDetailDto.Status = $"{item} was not found in section";
                    UpsertSimulationReportDetail(reportDetailDto);
                    _hubService.SendRealTimeMessage(_unitOfWork.CurrentUser?.Username, HubConstant.BroadcastReportGenerationStatus, reportDetailDto, simulationId);
                    Errors.Add(reportDetailDto.Status);
                    throw new KeyNotFoundException($"{item} was not found in section");
                }
            }
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
            Status = "Audit output report completed with errors";
            IsComplete = true;
        }

        private void checkCancelled(CancellationToken? cancellationToken, Guid simulationId)
        {
            if(cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
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
