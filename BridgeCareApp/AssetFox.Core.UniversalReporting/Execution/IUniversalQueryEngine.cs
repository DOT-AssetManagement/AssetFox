using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;

namespace AssetFox.Core.UniversalReporting.Execution;

public interface IUniversalQueryEngine
{
    Task<PagedViewerResult> ExecutePagedAsync(
        ReportComponent component,
        ViewerDataRequest request,
        int runId,
        CancellationToken cancellationToken);

    Task<ComponentResult> StreamAllAsync(
        ReportComponent component,
        ViewerDataRequest request,
        int runId,
        CancellationToken cancellationToken);
}
