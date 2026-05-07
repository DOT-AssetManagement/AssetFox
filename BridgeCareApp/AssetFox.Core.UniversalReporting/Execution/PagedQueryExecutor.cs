using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.UniversalReporting.Viewer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AssetFox.Core.UniversalReporting.Execution;

public class PagedQueryExecutor
{
    private readonly UnitOfDataPersistenceWork _unitOfWork;

    public PagedQueryExecutor(UnitOfDataPersistenceWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<PagedViewerResult> RunAsync(
        ComponentQuery query,
        ViewerDataRequest request,
        CancellationToken cancellationToken)
    {
        var rows = new List<List<object>>();
        var totalCount = 0;
        var connectionString = _unitOfWork.Context.Database.GetDbConnection().ConnectionString;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = query.Sql;
        command.CommandTimeout = 120;

        foreach (var parameter in query.Parameters)
        {
            command.Parameters.Add(CloneParameter(parameter));
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var totalCountOrdinal = TryGetOrdinal(reader, "TotalCount");

        while (await reader.ReadAsync(cancellationToken))
        {
            if (totalCountOrdinal >= 0 && !await reader.IsDBNullAsync(totalCountOrdinal, cancellationToken))
            {
                totalCount = reader.GetInt32(totalCountOrdinal);
            }

            rows.Add(ReadRow(reader, query.Columns));
        }

        if (totalCountOrdinal < 0)
        {
            totalCount = rows.Count;
        }

        return new PagedViewerResult
        {
            Columns = query.Columns.ToList(),
            Rows = rows,
            TotalRowCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    internal static List<object> ReadRow(SqlDataReader reader, IReadOnlyList<ViewerColumn> columns)
    {
        var row = new List<object>(columns.Count);
        foreach (var column in columns)
        {
            var ordinal = TryGetOrdinal(reader, MapColumnKeyToSqlAlias(column.Key));
            row.Add(ordinal < 0 || reader.IsDBNull(ordinal) ? null! : reader.GetValue(ordinal));
        }

        return row;
    }

    internal static SqlParameter CloneParameter(SqlParameter parameter) =>
        new(parameter.ParameterName, parameter.Value ?? DBNull.Value)
        {
            DbType = parameter.DbType,
            Direction = parameter.Direction,
            Size = parameter.Size,
            Precision = parameter.Precision,
            Scale = parameter.Scale
        };

    private static int TryGetOrdinal(SqlDataReader reader, string? fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return -1;
        }

        try
        {
            return reader.GetOrdinal(fieldName);
        }
        catch (IndexOutOfRangeException)
        {
            return -1;
        }
    }

    private static string MapColumnKeyToSqlAlias(string? key) =>
        key?.ToLowerInvariant() switch
        {
            "year" => "Year",
            "appliedtreatment" => "AppliedTreatment",
            "treatmentcause" => "TreatmentCause",
            "treatmentstatus" => "TreatmentStatus",
            "cost" => "Cost",
            "budget" => "Budget",
            "budgetname" => "BudgetName",
            "goalname" => "GoalName",
            _ => key ?? string.Empty
        };
}
