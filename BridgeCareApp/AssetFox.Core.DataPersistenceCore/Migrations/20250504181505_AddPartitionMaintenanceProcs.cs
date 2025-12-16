using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFox.Core.DataPersistenceCore.Migrations
{
    /// <inheritdoc />
    public partial class AddPartitionMaintenanceProcs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* ---------------------------------------------------------------- *
             * 1)  EnsureNextRunIdHasPartition  – JIT splitter                 *
             * ---------------------------------------------------------------- */
            migrationBuilder.Sql(@"
                EXEC(N'
                CREATE OR ALTER PROCEDURE dbo.usp_EnsureNextRunIdHasPartition
                    @RootTable         sysname      = N''dbo.SimulationOutput'',   -- identity table
                    @PartitionFunction sysname      = N''PF_SimulationRun'',
                    @ChunkSize         int          = 500,                        -- split in blocks
                    @MaxPartitions     int          = 15000                       -- hard cap
                AS
                BEGIN
                    SET NOCOUNT ON;

                    /* Next RunId SQL Server will assign */
                    DECLARE @nextRunId int =
                        IDENT_CURRENT(@RootTable) +
                        IDENT_INCR(@RootTable);

                    /* Current top boundary */
                    DECLARE @maxBoundary int =
                       (SELECT MAX(CONVERT(int,pv.value))
                        FROM   sys.partition_range_values pv
                        JOIN   sys.partition_functions pf
                               ON  pf.function_id = pv.function_id
                        WHERE  pf.name = @PartitionFunction);

                    IF (@nextRunId > @maxBoundary)
                    BEGIN
                        /* Safety: do not exceed @MaxPartitions */
                        DECLARE @currentParts int =
                           (SELECT COUNT(*)
                            FROM   sys.partition_range_values pv
                            JOIN   sys.partition_functions pf
                                   ON  pf.function_id = pv.function_id
                            WHERE  pf.name = @PartitionFunction);

                        IF @currentParts + @ChunkSize > @MaxPartitions
                            RAISERROR (''Partition limit of %d would be exceeded - aborting.'',
                                       16, 1, @MaxPartitions);

                        /* Split @ChunkSize new boundaries */
                        DECLARE @sql nvarchar(max) = N'''';
                        DECLARE @i int = 1;
                        WHILE @i <= @ChunkSize
                        BEGIN
                            SET @maxBoundary += 1;
                            SET @sql += N''ALTER PARTITION FUNCTION '' + QUOTENAME(@PartitionFunction) +
                                        N''() SPLIT RANGE ('' + CAST(@maxBoundary AS varchar(11)) + N'');'';
                            SET @i += 1;
                        END
                        EXEC (@sql);
                    END
                END
                ');
                ");

            /* ---------------------------------------------------------------- *
             * 2)  RecycleFreedRunPartition  – MERGE the empty boundary        *
             * ---------------------------------------------------------------- */
            migrationBuilder.Sql(@"
                EXEC(N'
                CREATE OR ALTER PROCEDURE dbo.usp_RecycleFreedRunPartition
                    @OldRunId          int,                       -- RunId just deleted
                    @PartitionFunction sysname = N''PF_SimulationRun''
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @merge nvarchar(max) =
                        N''ALTER PARTITION FUNCTION '' + QUOTENAME(@PartitionFunction) +
                        N''() MERGE RANGE ('' + CAST(@OldRunId AS varchar(11)) + N'');'';

                    EXEC (@merge);       -- execute the fully-assembled string
                END
                ');
                ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"IF OBJECT_ID(N'dbo.usp_EnsureNextRunIdHasPartition') IS NOT NULL
              DROP PROCEDURE dbo.usp_EnsureNextRunIdHasPartition;");

            migrationBuilder.Sql(
                @"IF OBJECT_ID(N'dbo.usp_RecycleFreedRunPartition') IS NOT NULL
              DROP PROCEDURE dbo.usp_RecycleFreedRunPartition;");
        }
    }
}
