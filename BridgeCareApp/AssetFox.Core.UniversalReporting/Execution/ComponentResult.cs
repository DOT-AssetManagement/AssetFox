using AssetFox.Core.UniversalReporting.Viewer;

namespace AssetFox.Core.UniversalReporting.Execution;

public sealed record ComponentResult(
    IReadOnlyList<ViewerColumn> Columns,
    IAsyncEnumerable<List<object>> Rows);
