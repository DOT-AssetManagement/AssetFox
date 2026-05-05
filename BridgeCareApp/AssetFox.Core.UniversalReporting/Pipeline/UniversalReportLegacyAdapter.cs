using AssetFox.Core.Common.Logging;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Reporting;
using AssetFox.Core.UniversalReporting.Charts;
using AssetFox.Core.UniversalReporting.Components;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Export;
using AssetFox.Core.UniversalReporting.Query;
using AssetFox.Core.UniversalReporting.Transforms;
using Newtonsoft.Json.Linq;

namespace AssetFox.Core.UniversalReporting.Pipeline;

public sealed class UniversalReportLegacyAdapter : IReport
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubService _hubService;

    public UniversalReportLegacyAdapter(
        IUnitOfWork unitOfWork,
        string reportTypeName,
        ReportIndexDTO? results,
        IHubService hubService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _hubService = hubService ?? throw new ArgumentNullException(nameof(hubService));

        ID = results?.Id ?? Guid.NewGuid();
        SimulationID = results?.SimulationId;
        NetworkID = results?.NetworkId;
        ReportTypeName = reportTypeName;
        Results = results?.Result ?? string.Empty;
        Errors = new List<string>();
        Status = "Report definition created.";
        IsComplete = !string.IsNullOrWhiteSpace(Results);
        Suffix = string.Empty;
        Criteria = string.Empty;
    }

    public Guid ID { get; set; }
    public Guid? SimulationID { get; set; }
    public Guid? NetworkID { get; set; }
    public string Results { get; private set; }
    public string Suffix { get; set; }
    public ReportType Type => ReportType.File;
    public string ReportTypeName { get; }
    public List<string> Errors { get; }
    public bool IsComplete { get; private set; }
    public string Status { get; private set; }
    public string Criteria { get; set; }

    public async Task Run(
        string parameters,
        CancellationToken? cancellationToken = null,
        IWorkQueueLog? workQueueLog = null)
    {
        var token = cancellationToken ?? CancellationToken.None;
        workQueueLog ??= new DoNothingWorkQueueLog();

        if (string.IsNullOrWhiteSpace(parameters))
        {
            IndicateError("Parameters string is empty OR there are no parameters defined");
            return;
        }

        JObject parameterObject;
        try
        {
            parameterObject = JObject.Parse(parameters);
        }
        catch (Exception ex)
        {
            IndicateError($"Failed to parse parameters: {ex.Message}");
            return;
        }

        if (!TryReadGuid(parameterObject, "reportId", out var reportId))
        {
            reportId = ID;
        }

        ID = reportId;

        if (!TryReadGuid(parameterObject, "simulationId", out var simulationId)
            && !TryReadGuid(parameterObject, "scenarioId", out simulationId))
        {
            IndicateError("Simulation ID could not be parsed to a Guid");
            return;
        }

        if (!TryReadGuid(parameterObject, "schemaId", out var schemaId))
        {
            IndicateError("Schema ID could not be parsed from parameters");
            return;
        }

        SimulationID = simulationId;

        try
        {
            token.ThrowIfCancellationRequested();

            var simulation = _unitOfWork.SimulationRepo.GetSimulation(simulationId);
            if (simulation == null)
            {
                IndicateError("Failed to find simulation");
                return;
            }

            NetworkID = simulation.NetworkId;
            Criteria = parameterObject.SelectToken("expression")?.ToString() ?? string.Empty;

            var username = _unitOfWork.CurrentUser?.Username ?? string.Empty;
            var progressReporter = new ExportProgressReporter(
                _unitOfWork,
                _hubService,
                workQueueLog,
                username,
                ID,
                simulationId,
                ReportTypeName);

            await progressReporter.ReportAsync("Generating universal report...", token);

            var runner = CreateRunner();
            Results = await runner.RunAsync(simulationId, schemaId, token, progressReporter);
            IsComplete = true;
            Status = "File generated.";

            await progressReporter.ReportAsync("Report Generation Completed", token);
        }
        catch (OperationCanceledException)
        {
            IndicateError("Report was cancelled");
        }
        catch (Exception ex)
        {
            IndicateError("Failed to generate universal report");
            Errors.Add(ex.Message);
        }
    }

    private UniversalReportRunner CreateRunner()
    {
        if (_unitOfWork is not UnitOfDataPersistenceWork unitOfDataPersistenceWork)
        {
            throw new InvalidOperationException(
                "Universal reporting requires UnitOfDataPersistenceWork.");
        }

        var criteriaEmitter = new CriteriaSqlEmitter();
        var chartValidator = new ChartTemplateValidator();
        var dataSources = new IComponentDataSource[]
        {
            new AssetDetailDataSource(unitOfDataPersistenceWork, criteriaEmitter),
            new TreatmentTimelineDataSource(unitOfDataPersistenceWork, criteriaEmitter),
            new ConditionTrackingDataSource(unitOfDataPersistenceWork, criteriaEmitter),
            new FundedTreatmentListDataSource(unitOfDataPersistenceWork, criteriaEmitter),
            new WorkSummaryDataSource(unitOfDataPersistenceWork, criteriaEmitter),
            new BudgetSummaryDataSource(unitOfDataPersistenceWork, criteriaEmitter),
            new TextBlockDataSource(unitOfDataPersistenceWork, criteriaEmitter),
            new BarChartDataSource(unitOfDataPersistenceWork, criteriaEmitter, chartValidator),
            new PieChartDataSource(unitOfDataPersistenceWork, criteriaEmitter, chartValidator),
            new LineChartDataSource(unitOfDataPersistenceWork, criteriaEmitter, chartValidator)
        };

        var registry = new ComponentDataSourceRegistry(dataSources);
        var pagedExecutor = new PagedQueryExecutor(unitOfDataPersistenceWork);
        var streamingExecutor = new StreamingQueryExecutor(unitOfDataPersistenceWork);
        var transformPipeline = new TransformPipeline(new IRowTransform[]
        {
            new PercentOfTotalTransform(),
            new TotalsRowTransform()
        });
        var queryEngine = new UniversalQueryEngine(
            unitOfDataPersistenceWork,
            registry,
            pagedExecutor,
            streamingExecutor,
            transformPipeline);
        var exporter = new UniversalReportExcelExporter(queryEngine);

        return new UniversalReportRunner(unitOfDataPersistenceWork, exporter);
    }

    private void IndicateError(string status)
    {
        Status = status;
        IsComplete = true;
        Errors.Add(status);
    }

    private static bool TryReadGuid(JObject parameterObject, string propertyName, out Guid value)
    {
        value = Guid.Empty;
        var token = parameterObject.SelectToken(propertyName);
        if (token == null)
        {
            return false;
        }

        if (token.Type == JTokenType.Array)
        {
            token = token.First;
        }

        return Guid.TryParse(token?.ToString(), out value);
    }
}
