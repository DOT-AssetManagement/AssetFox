using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.UniversalReporting.Charts;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Query;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;

namespace AssetFox.Core.UniversalReporting.Components;

public sealed class BarChartDataSource : ChartDataSourceBase, IComponentDataSource
{
    public BarChartDataSource(
        UnitOfDataPersistenceWork unitOfWork,
        CriteriaSqlEmitter criteriaEmitter,
        ChartTemplateValidator validator)
        : base(unitOfWork, criteriaEmitter, validator)
    {
    }

    public string ComponentType => "barChart";

    public ComponentQuery Build(
        ReportExecutionContext executionContext,
        ViewerDataRequest request,
        int runId,
        AttributeLookup attributes) =>
        BuildChartQuery(executionContext, request, runId, "bar");
}
