using System;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AssetFox.Core.DataUnitTests
{
    public static class DatabaseResetter
    {
        public static void ResetDatabase(UnitOfDataPersistenceWork unitOfWork)
        {
            var connectionString = unitOfWork.Context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("The database connection string is not configured.");

            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = connectionStringBuilder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new InvalidOperationException("The database name is not configured in the connection string.");
            }

            unitOfWork.Context.Database.CloseConnection();
            SqlConnection.ClearAllPools();

            connectionStringBuilder.InitialCatalog = "master";
            using (var connection = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandTimeout = (int)TimeSpan.FromMinutes(30).TotalSeconds;
                command.CommandText = $@"
IF DB_ID(N'{EscapeSqlLiteral(databaseName)}') IS NOT NULL
BEGIN
    ALTER DATABASE [{EscapeSqlIdentifier(databaseName)}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{EscapeSqlIdentifier(databaseName)}];
END;

CREATE DATABASE [{EscapeSqlIdentifier(databaseName)}];";
                command.ExecuteNonQuery();
            }

            SqlConnection.ClearAllPools();
            unitOfWork.Context.Database.Migrate();
        }

        private static string EscapeSqlIdentifier(string value) => value.Replace("]", "]]");

        private static string EscapeSqlLiteral(string value) => value.Replace("'", "''");
    }
}
