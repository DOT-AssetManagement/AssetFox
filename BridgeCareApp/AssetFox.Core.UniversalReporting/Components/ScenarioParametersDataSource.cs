using System.Runtime.CompilerServices;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Abstract;
using AssetFox.Core.UniversalReporting.Components.ScenarioParameters;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Query;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace AssetFox.Core.UniversalReporting.Components;

public sealed class ScenarioParametersDataSource : ComponentDataSourceBase, IComponentDataSource
{
    public ScenarioParametersDataSource(
        UnitOfDataPersistenceWork unitOfWork,
        CriteriaSqlEmitter criteriaEmitter)
        : base(unitOfWork, criteriaEmitter)
    {
    }

    public string ComponentType => "scenarioParameters";

    public ComponentQuery Build(
        ReportExecutionContext executionContext,
        ViewerDataRequest request,
        int runId,
        AttributeLookup attributes)
    {
        var config = executionContext.Component.Config ?? new Dictionary<string, object>();
        var selectedSections = ReadSelectedSections(config);

        return new ComponentQuery(
            string.Empty,
            Array.Empty<SqlParameter>(),
            ScenarioParametersDataContract.Columns,
            RequiresInMemoryPagination: true,
            cancellationToken => ReadRowsAsync(
                request.SimulationId,
                selectedSections,
                cancellationToken));
    }

    private async IAsyncEnumerable<List<object>> ReadRowsAsync(
        Guid simulationId,
        IReadOnlyCollection<string> selectedSections,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var source = new ScenarioParametersSourceData
        {
            Simulation = _unitOfWork.SimulationRepo.GetSimulation(simulationId),
            InvestmentPlan = _unitOfWork.InvestmentPlanRepo.GetInvestmentPlan(simulationId),
            AnalysisMethod = _unitOfWork.AnalysisMethodRepo.GetAnalysisMethod(simulationId),
            Budgets = _unitOfWork.BudgetRepo.GetScenarioBudgets(simulationId) ?? new List<BudgetDTO>(),
            BudgetPriorities = _unitOfWork.BudgetPriorityRepo.GetScenarioBudgetPriorities(simulationId) ?? new List<BudgetPriorityDTO>(),
            CashFlowRules = _unitOfWork.CashFlowRuleRepo.GetScenarioCashFlowRules(simulationId) ?? new List<CashFlowRuleDTO>(),
            Treatments = _unitOfWork.SelectableTreatmentRepo.GetScenarioSelectableTreatmentsWithCriterionLibrary(simulationId) ?? new List<TreatmentDTO>(),
            CommittedProjects = _unitOfWork.CommittedProjectRepo.GetCommittedProjectsForExport(simulationId) ?? new List<BaseCommittedProjectDTO>()
        };

        var document = ScenarioParametersDocumentBuilder.Build(source, selectedSections);
        foreach (var row in ScenarioParametersDataContract.Encode(document))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return row;
        }

        await Task.CompletedTask;
    }

    private static List<string> ReadSelectedSections(Dictionary<string, object> config)
    {
        if (!config.TryGetValue("sections", out var value) || value == null)
        {
            return ScenarioParameterSectionKeys.All.ToList();
        }

        if (value is JArray)
        {
            return GetConfigStringArray(config, "sections");
        }

        if (value is IEnumerable<string> strings)
        {
            return strings.Where(section => !string.IsNullOrWhiteSpace(section)).ToList();
        }

        if (value is IEnumerable<object> objects)
        {
            return objects
                .Select(Convert.ToString)
                .Where(section => !string.IsNullOrWhiteSpace(section))
                .Select(section => section!)
                .ToList();
        }

        return ScenarioParameterSectionKeys.All.ToList();
    }
}
