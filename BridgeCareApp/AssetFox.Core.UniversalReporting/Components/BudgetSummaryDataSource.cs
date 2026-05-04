using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Query;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AssetFox.Core.UniversalReporting.Components
{
    public class BudgetSummaryDataSource : ComponentDataSourceBase, IComponentDataSource
    {
        public BudgetSummaryDataSource(UnitOfDataPersistenceWork unitOfWork, CriteriaSqlEmitter criteriaEmitter)
            : base(unitOfWork, criteriaEmitter)
        {
        }

        public string ComponentType => "budgetSummary";

        public ComponentQuery Build(
            ReportComponent component,
            ViewerDataRequest request,
            int runId,
            AttributeLookup attributes)
        {
            var config = component.Config ?? new Dictionary<string, object>();
            var showSpent = GetConfigBool(config, "showSpentAmount", true);
            var showRemaining = GetConfigBool(config, "showRemainingAmount", true);
            var years = _unitOfWork.Context.SimulationYearDetail
                .AsNoTracking()
                .Where(y => y.RunId == runId)
                .OrderBy(y => y.Year)
                .Select(y => y.Year)
                .ToList();

            var columns = new List<ViewerColumnDTO>
            {
                new() { Key = "budgetName", Header = "Budget", Format = "text", Filterable = true, Sortable = true }
            };

            foreach (var year in years)
            {
                if (showSpent)
                {
                    columns.Add(new ViewerColumnDTO { Key = $"spent_{year}", Header = $"{year} Spent", Format = "currency", Filterable = false, Sortable = false });
                }

                if (showRemaining)
                {
                    columns.Add(new ViewerColumnDTO { Key = $"rem_{year}", Header = $"{year} Remaining", Format = "currency", Filterable = false, Sortable = false });
                }
            }

            return new ComponentQuery(
                string.Empty,
                Array.Empty<SqlParameter>(),
                columns.Select(ToViewerColumn).ToList(),
                RequiresInMemoryPagination: true,
                cancellationToken => ReadRowsAsync(request, runId, columns, years, showSpent, showRemaining, cancellationToken));
        }

        private async IAsyncEnumerable<List<object>> ReadRowsAsync(
            ViewerDataRequest request,
            int runId,
            List<ViewerColumnDTO> columns,
            List<int> years,
            bool showSpent,
            bool showRemaining,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var budgetData = await _unitOfWork.Context.BudgetDetail
                .AsNoTracking()
                .Where(b => b.RunId == runId)
                .Join(_unitOfWork.Context.SimulationYearDetail.AsNoTracking().Where(y => y.RunId == runId),
                    b => b.SimulationYearDetailId,
                    y => y.Id,
                    (b, y) => new { b.BudgetName, y.Year, b.AvailableFunding })
                .OrderBy(x => x.BudgetName)
                .ThenBy(x => x.Year)
                .ToListAsync(cancellationToken);

            var spendingData = new Dictionary<(string BudgetName, int Year), double>();
            if (showSpent)
            {
                await using var connection = new SqlConnection(_unitOfWork.Context.Database.GetDbConnection().ConnectionString);
                await connection.OpenAsync(cancellationToken);
                await using var command = connection.CreateCommand();
                command.CommandText = @"
SELECT a.BudgetName, a.Year, SUM(CAST(a.AllocatedAmount AS FLOAT)) AS TotalSpent
FROM dbo.Allocation a
WHERE a.RunId = @RunId
GROUP BY a.BudgetName, a.Year";
                command.CommandTimeout = 120;
                command.Parameters.Add(new SqlParameter("@RunId", runId));

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    var budgetName = reader.GetString(0);
                    var year = reader.GetInt32(1);
                    var totalSpent = reader.IsDBNull(2) ? 0.0 : reader.GetDouble(2);
                    spendingData[(budgetName, year)] = totalSpent;
                }
            }

            var colsPerYear = (showSpent ? 1 : 0) + (showRemaining ? 1 : 0);
            var yearIndexMap = years.Select((year, index) => new { year, index })
                .ToDictionary(item => item.year, item => item.index);
            var totalYearCols = years.Count * colsPerYear;
            var pivotData = new Dictionary<string, object[]>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in budgetData)
            {
                if (!pivotData.TryGetValue(item.BudgetName, out var yearValues))
                {
                    yearValues = Enumerable.Repeat<object>(0.0, totalYearCols).ToArray();
                    pivotData[item.BudgetName] = yearValues;
                }

                if (!yearIndexMap.TryGetValue(item.Year, out var yearIndex))
                {
                    continue;
                }

                var baseIndex = yearIndex * colsPerYear;
                var offset = 0;
                var spent = spendingData.TryGetValue((item.BudgetName, item.Year), out var spentValue) ? spentValue : 0.0;

                if (showSpent)
                {
                    yearValues[baseIndex + offset] = spent;
                    offset++;
                }

                if (showRemaining)
                {
                    yearValues[baseIndex + offset] = (double)item.AvailableFunding;
                }
            }

            var rows = pivotData
                .OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
                .Select(kvp =>
                {
                    var row = new List<object> { kvp.Key };
                    row.AddRange(kvp.Value);
                    return row;
                })
                .ToList();

            foreach (var row in ApplyInMemoryFilterAndSort(columns, rows, request))
            {
                yield return row;
            }
        }
    }
}
