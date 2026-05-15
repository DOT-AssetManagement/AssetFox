using AssetFox.Core.UniversalReporting.Viewer;
using Microsoft.Data.SqlClient;

namespace AssetFox.Core.UniversalReporting.Execution;

public sealed record ComponentQuery(
    string Sql,
    IReadOnlyList<SqlParameter> Parameters,
    IReadOnlyList<ViewerColumn> Columns,
    bool RequiresInMemoryPagination,
    Func<CancellationToken, IAsyncEnumerable<List<object>>>? CustomExecutor = null);
