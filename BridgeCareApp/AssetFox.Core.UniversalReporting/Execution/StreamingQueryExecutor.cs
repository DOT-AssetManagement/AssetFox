using System.Runtime.CompilerServices;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AssetFox.Core.UniversalReporting.Execution;

public class StreamingQueryExecutor
{
    private readonly UnitOfDataPersistenceWork _unitOfWork;

    public StreamingQueryExecutor(UnitOfDataPersistenceWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public IAsyncEnumerable<List<object>> RunAsync(
        ComponentQuery query,
        CancellationToken cancellationToken) =>
        ReadRowsAsync(query, cancellationToken);

    private async IAsyncEnumerable<List<object>> ReadRowsAsync(
        ComponentQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var connectionString = _unitOfWork.Context.Database.GetDbConnection().ConnectionString;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = query.Sql;
        command.CommandTimeout = 120;

        foreach (var parameter in query.Parameters)
        {
            command.Parameters.Add(PagedQueryExecutor.CloneParameter(parameter));
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return PagedQueryExecutor.ReadRow(reader, query.Columns);
        }
    }
}
