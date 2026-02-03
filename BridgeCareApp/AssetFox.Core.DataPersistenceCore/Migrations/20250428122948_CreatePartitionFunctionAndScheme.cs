using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFox.Core.DataPersistenceCore.Migrations
{
    /// <inheritdoc />
    public partial class CreatePartitionFunctionAndScheme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Generate boundary values from 1 to 1000
            // NOTE: For large numbers, generating this SQL string might be cumbersome.
            // Consider using a loop in C# to build the string or execute from a .sql file.
            var boundaryValues = string.Join(", ", Enumerable.Range(1, 1000)); // Creates "1, 2, 3, ..., 1000"

            migrationBuilder.Sql($@"
            IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = 'PF_SimulationRun')
                CREATE PARTITION FUNCTION PF_SimulationRun (INT)
                AS RANGE RIGHT FOR VALUES ({boundaryValues});
        ");
            // This creates 1001 partitions:
            // P1: <= 1
            // P2: = 2
            // P3: = 3
            // ...
            // P1001: = 1000
            // P1002: > 1000 (This last one is important!)

            migrationBuilder.Sql(@"
            IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = 'PS_SimulationRun')
                CREATE PARTITION SCHEME PS_SimulationRun
                AS PARTITION PF_SimulationRun ALL TO ([PRIMARY]); -- Or specific filegroups
        ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = 'PS_SimulationRun') DROP PARTITION SCHEME PS_SimulationRun;");
            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = 'PF_SimulationRun') DROP PARTITION FUNCTION PF_SimulationRun;");
        }
    }
}
