using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.UniversalReporting.Charts;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Query;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;

namespace AssetFox.Core.UniversalReporting.Components;

public sealed class LineChartDataSource : ChartDataSourceBase, IComponentDataSource
{
    public LineChartDataSource(
        UnitOfDataPersistenceWork unitOfWork,
        CriteriaSqlEmitter criteriaEmitter,
        ChartTemplateValidator validator)
        : base(unitOfWork, criteriaEmitter, validator)
    {
    }

    public string ComponentType => "lineChart";

    public ComponentQuery Build(
        ReportComponent component,
        ViewerDataRequest request,
        int runId,
        AttributeLookup attributes) =>
        BuildChartQuery(component, request, runId, "line");
}
