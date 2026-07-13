using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.UniversalReporting.Charts;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Query;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;

namespace AssetFox.Core.UniversalReporting.Components;

public sealed class PieChartDataSource : ChartDataSourceBase, IComponentDataSource
{
    public PieChartDataSource(
        UnitOfDataPersistenceWork unitOfWork,
        CriteriaSqlEmitter criteriaEmitter,
        ChartTemplateValidator validator)
        : base(unitOfWork, criteriaEmitter, validator)
    {
    }

    public string ComponentType => "pieChart";

    public ComponentQuery Build(
        ReportExecutionContext executionContext,
        ViewerDataRequest request,
        int runId,
        AttributeLookup attributes) =>
        BuildChartQuery(executionContext, request, runId, "pie");
}
