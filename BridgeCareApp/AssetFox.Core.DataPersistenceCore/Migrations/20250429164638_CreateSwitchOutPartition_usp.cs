using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFox.Core.DataPersistenceCore.Migrations
{
    /// <inheritdoc />
    public partial class CreateSwitchOutPartition_usp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                EXEC(N'
                CREATE OR ALTER PROCEDURE dbo.usp_PurgePartitionViaSwitchOut
                    @SourceTable          sysname,        -- e.g.  ''dbo.AssetSummaryDetail''
                    @PartitionValue       int,            -- value that maps to the partition
                    @PartitionFunction    sysname = N''PF_SimulationRun''
                AS
                BEGIN
                    SET NOCOUNT ON;

                    /* --------------------------------------------------
                       1. Resolve schema / base name and partition number
                    -------------------------------------------------- */
                    DECLARE @schema sysname = ISNULL(PARSENAME(@SourceTable,2),N''dbo'');
                    DECLARE @base   sysname =  PARSENAME(@SourceTable,1);
                    DECLARE @partNum int;

                    DECLARE @sql nvarchar(max) =
                      N''SELECT @partNum = $PARTITION.'' 
                      + QUOTENAME(@PartitionFunction)
                      + N''('' + CAST(@PartitionValue AS nvarchar(11)) + N'');'';

                    PRINT @sql;

                    EXEC sp_executesql
                     @sql,
                     N''@partNum int OUTPUT'',
                     @partNum OUTPUT;

                    /* --------------------------------------------------
                       2. Create an ad-hoc clone  <Base>_Delete_<GUID>
                    -------------------------------------------------- */
                    DECLARE @tmp  sysname = @base + N''_Delete_'' + LEFT(REPLACE(NEWID(),''-'',''''), 8);
                    DECLARE @fqTmp nvarchar(261) = QUOTENAME(@schema) + ''.'' + QUOTENAME(@tmp);
                    DECLARE @fqSrc nvarchar(261) = QUOTENAME(@schema) + ''.'' + QUOTENAME(@base);

                    /* 2b. All indexes, PK/UQ, FK –- reuse logic from staging proc
                           by calling it inline with the temp name */
                    EXEC dbo.usp_CreateStagingTableForPartitionedTable @SourceTable = @fqSrc, @Stage = @tmp;

                    /* --------------------------------------------------
                       3. Remove the placeholder CK (do not add a new one)
                    -------------------------------------------------- */
                    /* temp table is already partition-aligned;
                       extra CHECK constraints would block the switch */
                    DECLARE @dropCk nvarchar(max);
                    SELECT  @dropCk =
                             N''ALTER TABLE '' + @fqTmp +
                             N'' DROP CONSTRAINT '' + QUOTENAME(name) + N'';''
                    FROM    sys.check_constraints
                    WHERE   parent_object_id = OBJECT_ID(@fqTmp)
                      AND   definition LIKE N''(1%=%1)'';  -- our placeholder
                    
                    IF @dropCk IS NOT NULL
                         EXEC (@dropCk);
                    /* --------------------------------------------------
                       4. SWITCH the partition out and immediately drop it
                    -------------------------------------------------- */
                    DECLARE @sw nvarchar(max) =
                        N''ALTER TABLE ''+@fqSrc+N'' SWITCH PARTITION '' + CAST(@partNum AS varchar(10)) +
                        N'' TO '' + @fqTmp + N'' PARTITION '' + CAST(@partNum AS varchar(10)) + N'';'' +
                        CHAR(10) +
                        N''DROP TABLE ''+@fqTmp+N'';'';

                    EXEC(@sw);
                END
                ');
                ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF OBJECT_ID(N'dbo.usp_PurgePartitionViaSwitchOut') IS NOT NULL
                               DROP PROCEDURE dbo.usp_PurgePartitionViaSwitchOut;");
        }
    }
}
