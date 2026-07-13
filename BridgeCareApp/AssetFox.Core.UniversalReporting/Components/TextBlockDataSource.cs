using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Query;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;

namespace AssetFox.Core.UniversalReporting.Components
{
    public class TextBlockDataSource : ComponentDataSourceBase, IComponentDataSource
    {
        public TextBlockDataSource(UnitOfDataPersistenceWork unitOfWork, CriteriaSqlEmitter criteriaEmitter)
            : base(unitOfWork, criteriaEmitter)
        {
        }

        public string ComponentType => "textBlock";

        public ComponentQuery Build(
            ReportExecutionContext executionContext,
            ViewerDataRequest request,
            int runId,
            AttributeLookup attributes)
        {
            var component = executionContext.Component;
            var config = component.Config ?? new Dictionary<string, object>();
            var content = GetConfigString(config, "content", "");
            var style = GetConfigString(config, "style", "body");
            var columns = new List<ViewerColumnDTO>
            {
                new() { Key = "content", Header = "Content", Format = "text", Sortable = false, Filterable = false },
                new() { Key = "style", Header = "Style", Format = "text", Sortable = false, Filterable = false }
            };

            return new ComponentQuery(
                string.Empty,
                Array.Empty<Microsoft.Data.SqlClient.SqlParameter>(),
                columns.Select(ToViewerColumn).ToList(),
                RequiresInMemoryPagination: true,
                cancellationToken => ReadRowsAsync(content, style, cancellationToken));
        }

        private static async IAsyncEnumerable<List<object>> ReadRowsAsync(
            string content,
            string style,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new List<object> { content, style };
            await Task.CompletedTask;
        }
    }
}
