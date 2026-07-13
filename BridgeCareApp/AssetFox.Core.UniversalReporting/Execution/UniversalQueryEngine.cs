using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.UniversalReporting.Components;
using AssetFox.Core.UniversalReporting.Query;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Transforms;
using AssetFox.Core.UniversalReporting.Viewer;

namespace AssetFox.Core.UniversalReporting.Execution;

public class UniversalQueryEngine : IUniversalQueryEngine
{
    private readonly UnitOfDataPersistenceWork _unitOfWork;
    private readonly ComponentDataSourceRegistry _registry;
    private readonly PagedQueryExecutor _pagedExecutor;
    private readonly StreamingQueryExecutor _streamingExecutor;
    private readonly TransformPipeline _transformPipeline;

    public UniversalQueryEngine(
        UnitOfDataPersistenceWork unitOfWork,
        ComponentDataSourceRegistry registry,
        PagedQueryExecutor pagedExecutor,
        StreamingQueryExecutor streamingExecutor,
        TransformPipeline transformPipeline)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _pagedExecutor = pagedExecutor ?? throw new ArgumentNullException(nameof(pagedExecutor));
        _streamingExecutor = streamingExecutor ?? throw new ArgumentNullException(nameof(streamingExecutor));
        _transformPipeline = transformPipeline ?? throw new ArgumentNullException(nameof(transformPipeline));
    }

    public async Task<PagedViewerResult> ExecutePagedAsync(
        ReportExecutionContext executionContext,
        ViewerDataRequest request,
        int runId,
        CancellationToken cancellationToken)
    {
        var component = executionContext.Component;
        var dataSource = _registry.For(component.Type);
        var requiresTransforms = _transformPipeline.RequiresMaterialization(component);
        var buildRequest = requiresTransforms ? CreateAllRowsRequest(request) : request;
        var query = dataSource.Build(executionContext, buildRequest, runId, BuildAttributeLookup());

        if (query.CustomExecutor == null && !requiresTransforms)
        {
            return await _pagedExecutor.RunAsync(query, request, cancellationToken);
        }

        var rowStream = query.CustomExecutor?.Invoke(cancellationToken)
            ?? _streamingExecutor.RunAsync(query, cancellationToken);
        var materializedRows = await MaterializeAsync(rowStream, cancellationToken);
        var transformed = requiresTransforms
            ? _transformPipeline.Apply(component, query.Columns, materializedRows)
            : (query.Columns, materializedRows);

        var totalCount = transformed.Rows.Count;
        var offset = Math.Max(0, request.Page - 1) * request.PageSize;

        return new PagedViewerResult
        {
            Columns = transformed.Columns.ToList(),
            Rows = transformed.Rows.Skip(offset).Take(request.PageSize).ToList(),
            TotalRowCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public Task<ComponentResult> StreamAllAsync(
        ReportExecutionContext executionContext,
        ViewerDataRequest request,
        int runId,
        CancellationToken cancellationToken)
    {
        var component = executionContext.Component;
        var dataSource = _registry.For(component.Type);
        var query = dataSource.Build(executionContext, request, runId, BuildAttributeLookup());
        var rows = query.CustomExecutor?.Invoke(cancellationToken)
            ?? _streamingExecutor.RunAsync(query, cancellationToken);

        if (_transformPipeline.RequiresMaterialization(component))
        {
            return MaterializeAndTransformAsync(component, query.Columns, rows, cancellationToken);
        }

        return Task.FromResult(new ComponentResult(query.Columns, rows));
    }

    private AttributeLookup BuildAttributeLookup() =>
        new(_unitOfWork.AttributeRepo.GetAllAttributesAbbreviated());

    private static async Task<List<List<object>>> MaterializeAsync(
        IAsyncEnumerable<List<object>> rows,
        CancellationToken cancellationToken)
    {
        var materialized = new List<List<object>>();
        await foreach (var row in rows.WithCancellation(cancellationToken))
        {
            materialized.Add(row);
        }

        return materialized;
    }

    private async Task<ComponentResult> MaterializeAndTransformAsync(
        ReportComponent component,
        IReadOnlyList<ViewerColumn> columns,
        IAsyncEnumerable<List<object>> rows,
        CancellationToken cancellationToken)
    {
        var materializedRows = await MaterializeAsync(rows, cancellationToken);
        var transformed = _transformPipeline.Apply(component, columns, materializedRows);
        return new ComponentResult(transformed.Columns, YieldRows(transformed.Rows));
    }

    private static async IAsyncEnumerable<List<object>> YieldRows(
        IReadOnlyList<List<object>> rows)
    {
        foreach (var row in rows)
        {
            yield return row;
            await Task.Yield();
        }
    }

    private static ViewerDataRequest CreateAllRowsRequest(ViewerDataRequest request) =>
        new()
        {
            SimulationId = request.SimulationId,
            Page = 1,
            PageSize = int.MaxValue,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            ColumnFilters = new Dictionary<string, string>(request.ColumnFilters)
        };
}
