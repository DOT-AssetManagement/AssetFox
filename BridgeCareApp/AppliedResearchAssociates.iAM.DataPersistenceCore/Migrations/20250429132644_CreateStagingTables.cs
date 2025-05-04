using System;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Migrations
{
    /// <inheritdoc />
    public partial class CreateStagingTables : Migration
    {
        // List *base* names of partitioned tables
        private readonly string[] tableNames = {
        "SimulationYearDetail", "AssetSummaryDetail", "AssetDetail",
        "AssetSummaryDetailValueIntId", "AssetDetailValueIntId",
        "TreatmentOptionDetail", "TreatmentRejectionDetail", "TreatmentSchedulingCollisionDetail",
        "TreatmentConsiderationDetail", "FundingCalculationInput", "BudgetToSpend", // Reordered based on FKs
        "FundingCalculationOutput", "Allocation", "CashFlowConsiderationDetail", "TargetConditionGoalDetail", "DeficientConditionGoalDetail", "BudgetDetail"
        };

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------------------------------------------------------------------------------
            // 1)  CREATE OR ALTER PROCEDURE dbo.usp_CreateStagingTableForPartitionedTable
            // ---------------------------------------------------------------------------------
            migrationBuilder.Sql(@"
                EXEC (N'
                CREATE OR ALTER PROCEDURE dbo.usp_CreateStagingTableForPartitionedTable
                    @SourceTable SYSNAME,           -- e.g.  ''dbo.MyTable''
                    @Stage       sysname = NULL         -- OPTIONAL: override the staging name
                AS
                BEGIN
                    SET NOCOUNT ON;

                    /* --------------------------------------------------------- *
                     * 1. Parse parts & bail if the staging table already exists *
                     * --------------------------------------------------------- */
                    DECLARE @Schema SYSNAME = ISNULL(PARSENAME(@SourceTable,2), ''dbo'');
                    DECLARE @Base   SYSNAME =  PARSENAME(@SourceTable,1);

                    IF @Stage IS NULL
                        SET @Stage = @Base + N''_Staging'';

                    IF OBJECT_ID(QUOTENAME(@Schema)+''.''+QUOTENAME(@Stage)) IS NOT NULL
                        RETURN;

                    /* --------------------------- *
                     * 2. Clone the base structure *
                     * --------------------------- */
                    DECLARE @sql NVARCHAR(MAX) =
                        N''SELECT TOP(0) * INTO ''+QUOTENAME(@Schema)+''.''+QUOTENAME(@Stage)+
                        N'' FROM ''+QUOTENAME(@Schema)+''.''+QUOTENAME(@Base)+N'';'';
                    EXEC(@sql);

                    /* ------------------------------------------------ *
                     * 3. Re-create every NON-PK index from the source  *
                     * ------------------------------------------------ */
                DECLARE
                        @iname     sysname,
                        @uniq      bit,
                        @type_desc nvarchar(60),
                        @filter    nvarchar(max),
                        @dspace    sysname,
                        @ds_type   nvarchar(60),
                        @key_cols  nvarchar(max),
                        @inc_cols  nvarchar(max),
                        @part_cols nvarchar(max),
                        @idSql       nvarchar(max);

                DECLARE idx CURSOR LOCAL FAST_FORWARD FOR
                SELECT  i.name,
                        i.is_unique,
                        i.type_desc,
                        i.filter_definition,
                        ds.name,
                        ds.type_desc,
                        -- key cols
                        STUFF((
                            SELECT N'', ''+QUOTENAME(c2.name)
                            FROM   sys.index_columns ic2
                            JOIN   sys.columns c2
                                   ON  c2.object_id = ic2.object_id
                                  AND c2.column_id  = ic2.column_id
                            WHERE  ic2.object_id = i.object_id
                              AND  ic2.index_id  = i.index_id
                              AND  ic2.is_included_column = 0
                              AND  ic2.key_ordinal > 0
                            ORDER BY ic2.key_ordinal
                            FOR XML PATH(N''''), TYPE).value(N''.'',N''nvarchar(max)''),1,2,N'''')  AS key_cols,
                        -- include cols
                        NULLIF(
                        STUFF((
                            SELECT N'', ''+QUOTENAME(c3.name)
                            FROM   sys.index_columns ic3
                            JOIN   sys.columns c3
                                   ON  c3.object_id = ic3.object_id
                                  AND c3.column_id  = ic3.column_id
                            WHERE  ic3.object_id = i.object_id
                              AND  ic3.index_id  = i.index_id
                              AND  ic3.is_included_column = 1
                            ORDER BY c3.name
                            FOR XML PATH(N''''), TYPE).value(N''.'',N''nvarchar(max)''),1,2,N''''),N'''') AS inc_cols,
                        -- partition column list (if any)
                        NULLIF(
                        STUFF((
                            SELECT N'', ''+QUOTENAME(c4.name)
                            FROM   sys.index_columns ic4
                            JOIN   sys.columns c4
                                   ON  c4.object_id = ic4.object_id
                                  AND c4.column_id  = ic4.column_id
                            WHERE  ic4.object_id = i.object_id
                              AND  ic4.index_id  = i.index_id
                              AND  ic4.partition_ordinal > 0
                            ORDER BY ic4.partition_ordinal
                            FOR XML PATH(N''''), TYPE).value(N''.'',N''nvarchar(max)''),1,2,N''''),N'''') AS part_cols
                FROM   sys.indexes i
                JOIN   sys.data_spaces ds ON ds.data_space_id = i.data_space_id
                WHERE  i.object_id = OBJECT_ID(QUOTENAME(@Schema)+N''.''+QUOTENAME(@Base))
                  AND  i.is_hypothetical = 0
                  AND  i.index_id NOT IN (          --skip indexes that back PK / UQ
                    SELECT kc.unique_index_id
                    FROM   sys.key_constraints kc
                    WHERE  kc.parent_object_id = i.object_id );

                OPEN idx;
                FETCH NEXT FROM idx INTO @iname,@uniq,@type_desc,@filter,@dspace,@ds_type,
                                         @key_cols,@inc_cols,@part_cols;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    /* build one CREATE INDEX statement */
                    SET @idSql = N''CREATE ''+
                               CASE WHEN @uniq = 1 THEN N''UNIQUE '' ELSE N'''' END +
                               @type_desc + N'' INDEX ''+QUOTENAME(@iname)+N''
                               ON ''+QUOTENAME(@Schema)+N''.''+QUOTENAME(@Stage)+
                               N'' (''+@key_cols+N'')'' +
                               CASE WHEN @inc_cols IS NOT NULL
                                    THEN N'' INCLUDE (''+@inc_cols+N'')'' ELSE N'''' END +
                               CASE WHEN @filter IS NOT NULL
                                    THEN N'' WHERE ''+@filter ELSE N'''' END +
                               N'' ON ''+QUOTENAME(@dspace) +
                               CASE WHEN @ds_type = N''PARTITION_SCHEME''
                                     AND   @part_cols IS NOT NULL
                                     THEN N''(''+@part_cols+N'')'' ELSE N'''' END +
                               N'';'';

                    EXEC (@idSql);                     -- create that index
                    FETCH NEXT FROM idx INTO @iname,@uniq,@type_desc,@filter,@dspace,@ds_type,
                                             @key_cols,@inc_cols,@part_cols;
                END
                CLOSE idx; DEALLOCATE idx;

                /* ======================================================= *
                 * 4.  Re-create PRIMARY KEY  and UNIQUE constraints        *
                 *     (foreign keys are still skipped)                     *
                 * ======================================================= */
                DECLARE
                        @cName   sysname,
                        @newName sysname,
                        @cType   nchar(2),          -- ''PK'' or ''UQ''
                        @cIsClst bit,
                        @cCols   nvarchar(max),
                        @cSql    nvarchar(max);

                DECLARE c CURSOR LOCAL FAST_FORWARD FOR
                SELECT kc.name,
                       kc.type,                     -- ''PK'' / ''UQ''
                       CASE idx.type WHEN 1 THEN 1 ELSE 0 END,   -- clustered?
                       STUFF((
                           SELECT N'', '' + QUOTENAME(c.name) +
                                  CASE WHEN ic.is_descending_key = 1 THEN N'' DESC'' ELSE N'' ASC'' END
                           FROM   sys.index_columns ic
                           JOIN   sys.columns       c
                                  ON  c.object_id = ic.object_id
                                 AND c.column_id  = ic.column_id
                           WHERE  ic.object_id = kc.parent_object_id
                             AND  ic.index_id  = kc.unique_index_id
                             AND  ic.is_included_column = 0
                           ORDER BY ic.key_ordinal
                           FOR XML PATH(N''''), TYPE).value(N''.'', N''nvarchar(max)''), 1, 2, N'''')
                FROM   sys.key_constraints kc
                JOIN   sys.indexes         idx
                       ON  idx.object_id = kc.parent_object_id
                      AND idx.index_id   = kc.unique_index_id
                WHERE  kc.parent_object_id = OBJECT_ID(QUOTENAME(@Schema)+N''.''+QUOTENAME(@Base));

                OPEN c;
                FETCH NEXT FROM c INTO @cName,@cType,@cIsClst,@cCols;

                WHILE @@FETCH_STATUS = 0
                BEGIN
                    /* make the name unique in this database */
                    /* Unique, readable name:  <Orig>_on_<TargetBase>  (trim to 128) */
                    SET @newName = LEFT(
                        @cName + N''_on_'' + @Stage,          -- @Stage is the target base name
                        128);

                    SET @cSql = N''ALTER TABLE ''+QUOTENAME(@Schema)+N''.''+QUOTENAME(@Stage)+
                               N'' ADD CONSTRAINT ''+QUOTENAME(@newName)+N'' '' +
                               CASE WHEN @cType = N''PK''
                                        THEN N''PRIMARY KEY ''
                                        ELSE N''UNIQUE '' END +
                               CASE WHEN @cIsClst = 1 THEN N''CLUSTERED '' ELSE N''NONCLUSTERED '' END +
                               N''(''+@cCols+N'');'';

                    EXEC (@cSql);

                    FETCH NEXT FROM c INTO @cName,@cType,@cIsClst,@cCols;
                END
                CLOSE c; DEALLOCATE c;

                /* =================================================== *
                 * 5.  Re-create FOREIGN KEY constraints (trusted)      *
                 * =================================================== */
                DECLARE
                        @fkName  sysname,
                        @srcCols nvarchar(max),
                        @refTbl  nvarchar(max),
                        @refCols nvarchar(max),
                        @fkSql   nvarchar(max);

                DECLARE fk CURSOR LOCAL FAST_FORWARD FOR
                SELECT fk.name,
                       -- referencing columns
                       STUFF((
                           SELECT N'', '' + QUOTENAME(c1.name)
                           FROM   sys.foreign_key_columns fkc1
                           JOIN   sys.columns c1
                                  ON c1.object_id = fkc1.parent_object_id
                                 AND c1.column_id = fkc1.parent_column_id
                           WHERE  fkc1.constraint_object_id = fk.object_id
                           ORDER BY fkc1.constraint_column_id
                           FOR XML PATH(N''''), TYPE).value(N''.'',N''nvarchar(max)''),1,2,N'''') AS src_cols,
                       -- referenced table
                       QUOTENAME(SCHEMA_NAME(rt.schema_id)) + N''.'' + QUOTENAME(rt.name)        AS ref_table,
                       -- referenced columns
                       STUFF((
                           SELECT N'', '' + QUOTENAME(c2.name)
                           FROM   sys.foreign_key_columns fkc2
                           JOIN   sys.columns c2
                                  ON c2.object_id = fkc2.referenced_object_id
                                 AND c2.column_id = fkc2.referenced_column_id
                           WHERE  fkc2.constraint_object_id = fk.object_id
                           ORDER BY fkc2.constraint_column_id
                           FOR XML PATH(N''''), TYPE).value(N''.'',N''nvarchar(max)''),1,2,N'''') AS ref_cols
                FROM   sys.foreign_keys fk
                JOIN   sys.tables       rt ON rt.object_id = fk.referenced_object_id
                WHERE  fk.parent_object_id = OBJECT_ID(QUOTENAME(@Schema)+N''.''+QUOTENAME(@Base));

                OPEN fk;
                FETCH NEXT FROM fk INTO @fkName,@srcCols,@refTbl,@refCols;

                WHILE @@FETCH_STATUS = 0
                BEGIN
                    /* ensure unique name */
                    DECLARE @newFkName sysname =
                        LEFT(@fkName + N''_on_'' + @Stage, 128);

                    /* trusted FK: WITH CHECK + second CHECK to mark it trusted */
                    SET @fkSql =
                        N''ALTER TABLE '' + QUOTENAME(@Schema) + N''.'' + QUOTENAME(@Stage) +
                        N''  WITH CHECK ADD CONSTRAINT '' + QUOTENAME(@newFkName) +
                        N'' FOREIGN KEY ('' + @srcCols + N'')'' +
                        N'' REFERENCES '' + @refTbl + N'' ('' + @refCols + N'');'' +
                        N'' ALTER TABLE '' + QUOTENAME(@Schema) + N''.'' + QUOTENAME(@Stage) +
                        N''  CHECK CONSTRAINT '' + QUOTENAME(@newFkName) + N'';'';   -- ensure trusted

                    EXEC (@fkSql);

                    FETCH NEXT FROM fk INTO @fkName,@srcCols,@refTbl,@refCols;
                END
                CLOSE fk; DEALLOCATE fk;
                    /* -------------------------------- *
                     * 6. Placeholder check constraint  *
                     * -------------------------------- */
                    DECLARE @ckSql nvarchar(max) =
                      N''ALTER TABLE ''
                    + QUOTENAME(@Schema) + N''.'' + QUOTENAME(@Stage) +
                      N'' WITH NOCHECK
                         ADD CONSTRAINT CK_'' + @Stage + N''_AlwaysTrue
                         CHECK (1 = 1);'';

                    EXEC (@ckSql);
                END
                ');
                ");

            // ---------------------------------------------------------------------
            // 2)  CALL THE PROC FOR EACH PARTITIONED TABLE YOU CARE ABOUT
            // ---------------------------------------------------------------------

            foreach (var table in tableNames)
            {
                migrationBuilder.Sql($@"EXEC dbo.usp_CreateStagingTableForPartitionedTable N'{table}';");
            }
        }

        // -------------------------------------------------------------------------
        // 3)  DOWN  – clean up everything we created in Up
        // -------------------------------------------------------------------------
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            // Drop the staging tables
            foreach (var table in tableNames)
            {
                var schema = table.Contains('.')
                           ? table.Split('.')[0]
                           : "dbo";

                var baseName = table.Contains('.')
                             ? table.Split('.')[1]
                             : table;

                var staging = $"{baseName}_Staging";

                migrationBuilder.Sql($@"
IF OBJECT_ID(N'[{schema}].[{staging}]') IS NOT NULL
    DROP TABLE [{schema}].[{staging}];");
            }

            // Finally drop the stored procedure
            migrationBuilder.Sql(@"IF OBJECT_ID(N'dbo.usp_CreateStagingTableForPartitionedTable') IS NOT NULL
                               DROP PROCEDURE dbo.usp_CreateStagingTableForPartitionedTable;");
        }
    }
}
