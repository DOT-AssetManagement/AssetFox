using System;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFox.Core.DataPersistenceCore.Migrations
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

                    DECLARE @BaseObjectId INT = OBJECT_ID(QUOTENAME(@Schema)+N''.''+QUOTENAME(@Base));
                    IF @BaseObjectId IS NULL
                    BEGIN
                        -- Handle error: Source table not found
                        RETURN;
                    END

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

                    /* =================================================== *
                     * 3.  Re-create DEFAULT constraints                  *
                     * =================================================== */
                    DECLARE @dcName sysname, @dcColumnName sysname, @dcDefinition nvarchar(max), @dcSql nvarchar(max);

                    DECLARE dc CURSOR LOCAL FAST_FORWARD FOR
                    SELECT
                        dc.name AS DefaultConstraintName,
                        c.name AS ColumnName,
                        dc.definition AS Definition
                    FROM sys.default_constraints dc
                    JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
                    WHERE dc.parent_object_id = @BaseObjectId;

                    OPEN dc;
                    FETCH NEXT FROM dc INTO @dcName, @dcColumnName, @dcDefinition;
                    WHILE @@FETCH_STATUS = 0
                    BEGIN
                        DECLARE @newDcName sysname = N''DF_'' + @Stage + N''_'' + @dcColumnName; 
                        -- You might want a more robust unique naming, but this is common. 
                        -- Or try to keep original name if it doesn''t clash: LEFT(@dcName + N''_on_'' + @Stage, 128)

                        SET @dcSql = N''ALTER TABLE '' + QUOTENAME(@Schema) + N''.'' + QUOTENAME(@Stage) +
                                     N'' ADD CONSTRAINT '' + QUOTENAME(@newDcName) +
                                     N'' DEFAULT '' + @dcDefinition + 
                                     N'' FOR '' + QUOTENAME(@dcColumnName) + N'';'';
                        EXEC (@dcSql);
                        FETCH NEXT FROM dc INTO @dcName, @dcColumnName, @dcDefinition;
                    END
                    CLOSE dc; DEALLOCATE dc;

                    /* -------------------------------------------------------------------- *
                     * 4.  Re-create PRIMARY KEY and UNIQUE constraints (partition-aware)   *
                     * -------------------------------------------------------------------- */
                    DECLARE
                            @cName     sysname,
                            @newName   sysname,
                            @cType     nchar(2),     -- ''PK'' / ''UQ''
                            @cIsClst   bit,
                            @cCols     nvarchar(max),
                            @dspace2    sysname,
                            @ds_type2   nvarchar(60), -- ''PARTITION_SCHEME'' | ''FG'' | …
                            @part_cols2 nvarchar(max),
                            @cSql      nvarchar(max);

                    DECLARE c CURSOR LOCAL FAST_FORWARD FOR
                    SELECT kc.name,
                           kc.type,                           -- ''PK'' / ''UQ''
                           CASE idx.type WHEN 1 THEN 1 ELSE 0 END,        -- clustered?
                           -- key columns
                           STUFF((
                               SELECT '', '' + QUOTENAME(c.name) +
                                      CASE WHEN ic.is_descending_key = 1
                                           THEN '' DESC'' ELSE '' ASC'' END
                               FROM   sys.index_columns ic
                               JOIN   sys.columns       c
                                      ON c.object_id = ic.object_id
                                     AND c.column_id  = ic.column_id
                               WHERE  ic.object_id = kc.parent_object_id
                                 AND  ic.index_id  = kc.unique_index_id
                                 AND  ic.is_included_column = 0
                               ORDER BY ic.key_ordinal
                               FOR XML PATH(''''), TYPE).value(''.'', ''nvarchar(max)''), 1, 2, '''') AS key_cols,
                           ds.name,
                           ds.type_desc,
                           -- partition column list (if any)
                           NULLIF(
                               STUFF((
                                   SELECT '', '' + QUOTENAME(c4.name)
                                   FROM   sys.index_columns ic4
                                   JOIN   sys.columns      c4
                                          ON c4.object_id = ic4.object_id
                                         AND c4.column_id = ic4.column_id
                                   WHERE  ic4.object_id = kc.parent_object_id
                                     AND  ic4.index_id  = kc.unique_index_id
                                     AND  ic4.partition_ordinal > 0
                                   ORDER BY ic4.partition_ordinal
                                   FOR XML PATH(''''), TYPE).value(''.'', ''nvarchar(max)''), 1, 2, ''''), '''') AS part_cols2
                    FROM   sys.key_constraints kc
                    JOIN   sys.indexes         idx
                           ON idx.object_id = kc.parent_object_id
                          AND idx.index_id   = kc.unique_index_id
                    JOIN   sys.data_spaces     ds
                           ON ds.data_space_id = idx.data_space_id
                    WHERE  kc.parent_object_id = OBJECT_ID(QUOTENAME(@Schema)+''.''+QUOTENAME(@Base));

                    OPEN  c;
                    FETCH NEXT FROM c INTO @cName,@cType,@cIsClst,@cCols,
                                           @dspace2,@ds_type2,@part_cols2;

                    WHILE @@FETCH_STATUS = 0
                    BEGIN
                        /* unique, deterministic name */
                        SET @newName = LEFT(@cName + N''_on_'' + @Stage, 128);

                        SET @cSql = N''ALTER TABLE '' + QUOTENAME(@Schema) + ''.'' + QUOTENAME(@Stage) +
                                   N'' ADD CONSTRAINT '' + QUOTENAME(@newName) + N'' '' +
                                   CASE WHEN @cType = N''PK'' THEN N''PRIMARY KEY '' ELSE N''UNIQUE '' END +
                                   CASE WHEN @cIsClst = 1  THEN N''CLUSTERED '' ELSE N''NONCLUSTERED '' END +
                                   ''('' + @cCols + N'')'' +
                                   CASE
                                     WHEN @ds_type2 = N''PARTITION_SCHEME''
                                     THEN N'' ON '' + QUOTENAME(@dspace2) +
                                          N''('' + ISNULL(@part_cols2, '''') + N'')''
                                     ELSE N''''  -- filegroup index, leave as default
                                   END + N'';'';

                        EXEC (@cSql);

                        FETCH NEXT FROM c INTO @cName,@cType,@cIsClst,@cCols,
                                               @dspace2,@ds_type2,@part_cols2;
                    END
                    CLOSE c; DEALLOCATE c;

                    /* ------------------------------------------------ *
                     * 5. Re-create every NON-PK index from the source  *
                     * ------------------------------------------------ */
                    DECLARE
                            @iname     sysname,
                            @uniq      bit,
                            @type_desc nvarchar(60),
                            @filter    nvarchar(max),
                            @dspace    sysname,
                            @ds_type   nvarchar(60),
                            @key_cols  nvarchar(max),
                            @key_cols_src  nvarchar(max),
                            @inc_cols  nvarchar(max),
                            @inc_cols_src  nvarchar(max),
                            @part_cols nvarchar(max),
                            @part_cols_src nvarchar(max),
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
                                @key_cols_src,@inc_cols_src,@part_cols_src;

                    WHILE @@FETCH_STATUS = 0
                    BEGIN
                        SET @idSql = N''''; -- Initialize @idSql

                        IF @type_desc = N''NONCLUSTERED COLUMNSTORE''
                        BEGIN
                            DECLARE @ncc_cols NVARCHAR(MAX);
                            DECLARE @CurrentIndexId INT;

                            -- Get the index_id of the current NCCI on the source table
                            SELECT @CurrentIndexId = src_idx.index_id
                            FROM sys.indexes src_idx
                            WHERE src_idx.name = @iname AND src_idx.object_id = @BaseObjectId;

                            IF @CurrentIndexId IS NULL
                            BEGIN
                                SET @idSql = N''-- ERROR: Could not find source index '' + QUOTENAME(@iname) + N'' on table '' + QUOTENAME(@SourceTable);
                            END
                            ELSE
                            BEGIN
                                -- Get ALL columns for the NCCI from sys.index_columns of the source table
                                -- This list MUST include the partitioning key column (e.g., RunId)
                                SELECT @ncc_cols = STUFF((
                                    SELECT N'', '' + QUOTENAME(c.name)
                                    FROM sys.index_columns ic
                                    JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                                    WHERE ic.object_id = @BaseObjectId -- Source Table Object ID
                                      AND ic.index_id = @CurrentIndexId
                                    ORDER BY ic.index_column_id -- Preserves the original column order of the NCCI
                                    FOR XML PATH(N''''), TYPE).value(N''.'', N''nvarchar(max)''), 1, 2, N'''');

                                IF @ncc_cols IS NULL OR @ncc_cols = N''''
                                BEGIN
                                    SET @idSql = N''-- ERROR: NCCI '' + QUOTENAME(@iname) + N'' has no columns defined.'';
                                END
                                ELSE
                                BEGIN
                                    SET @idSql = N''CREATE NONCLUSTERED COLUMNSTORE INDEX '' + QUOTENAME(@iname) +
                                                 N'' ON '' + QUOTENAME(@Schema) + N''.'' + QUOTENAME(@Stage) +
                                                 N'' ('' + @ncc_cols + N'')'';

                                    IF @filter IS NOT NULL AND @filter <> N'''' -- Check if filter definition exists
                                    BEGIN
                                        SET @idSql = @idSql + N'' WHERE '' + @filter;
                                    END

                                    -- Append partitioning clause.
                                    -- @dspace (from cursor) = Name of the data space (partition scheme or filegroup)
                                    -- @ds_type (from cursor) = Type of data space (''PARTITION_SCHEME'', ''ROWS_FILEGROUP'')
                                    -- @part_cols_src (from cursor) = Partitioning column(s) for the index (e.g., ''RunId'')

                                    IF @dspace IS NOT NULL AND @dspace <> N''''
                                    BEGIN
                                        SET @idSql = @idSql + N'' ON '' + QUOTENAME(@dspace);
                                        IF @ds_type = N''PARTITION_SCHEME'' AND @part_cols_src IS NOT NULL AND @part_cols_src <> N''''
                                        BEGIN
                                            SET @idSql = @idSql + N''('' + @part_cols_src + N'')'';
                                        END
                                        -- ELSE: If it''s not a partition scheme but a filegroup, no column list in parentheses for ON clause.
                                    END
                                    ELSE
                                    BEGIN
                                        -- This case should ideally not be hit if source NCCI is on a partition scheme or specific filegroup.
                                        -- It might fall back to database default filegroup if @dspace is null.
                                        -- For a partitioned base table, NCCI must be partitioned, so @dspace MUST be the partition scheme.
                                        SET @idSql = N''-- ERROR: Data space not determined for NCCI '' + QUOTENAME(@iname) + N''. Partition alignment will likely fail.'';
                                    END
                                    SET @idSql = @idSql + N'';'';
                                END
                            END
                        END
                        ELSE
                        BEGIN
                            -- Existing logic for rowstore indexes (using @key_cols_src, @inc_cols_src)
                            -- Make sure to use the variables fetched from the cursor, e.g., @key_cols_src and @inc_cols_src
                            -- Ensure you re-fetch @key_cols and @inc_cols using the original logic if they are not directly available
                            -- For simplicity, assuming you have correctly populated @key_cols and @inc_cols from the cursor for rowstore
                            DECLARE @current_key_cols NVARCHAR(MAX);
                            DECLARE @current_inc_cols NVARCHAR(MAX);

                            SELECT @current_key_cols = STUFF((
                                SELECT N'', '' + QUOTENAME(c2.name) + CASE WHEN ic2.is_descending_key = 1 THEN N'' DESC'' ELSE N'' ASC'' END
                                FROM sys.index_columns ic2
                                JOIN sys.columns c2 ON c2.object_id = ic2.object_id AND c2.column_id = ic2.column_id
                                WHERE ic2.object_id = @BaseObjectId
                                  AND ic2.index_id = (SELECT idx_sub.index_id FROM sys.indexes idx_sub WHERE idx_sub.name = @iname AND idx_sub.object_id = @BaseObjectId)
                                  AND ic2.is_included_column = 0 AND ic2.key_ordinal > 0
                                ORDER BY ic2.key_ordinal
                                FOR XML PATH(N''''), TYPE).value(N''.'',N''nvarchar(max)''),1,2,N'''');

                            SELECT @current_inc_cols = NULLIF(STUFF((
                                SELECT N'', '' + QUOTENAME(c3.name)
                                FROM sys.index_columns ic3
                                JOIN sys.columns c3 ON c3.object_id = ic3.object_id AND c3.column_id  = ic3.column_id
                                WHERE ic3.object_id = @BaseObjectId
                                  AND ic3.index_id = (SELECT idx_sub.index_id FROM sys.indexes idx_sub WHERE idx_sub.name = @iname AND idx_sub.object_id = @BaseObjectId)
                                  AND ic3.is_included_column = 1
                                ORDER BY c3.name -- or ic3.index_column_id
                                FOR XML PATH(N''''), TYPE).value(N''.'',N''nvarchar(max)''),1,2,N''''),N'''');

                            SET @idSql = N''CREATE ''+
                                       CASE WHEN @uniq = 1 THEN N''UNIQUE '' ELSE N'''' END +
                                       @type_desc + N'' INDEX ''+QUOTENAME(@iname)+N''
                                       ON ''+QUOTENAME(@Schema)+N''.''+QUOTENAME(@Stage)+
                                       N'' (''+ ISNULL(@current_key_cols, N'''') +N'')'' + -- ISNULL for safety if an index has no key columns (e.g. XML index primary)
                                       CASE WHEN @current_inc_cols IS NOT NULL THEN N'' INCLUDE (''+ @current_inc_cols +N'')'' ELSE N'''' END +
                                       CASE WHEN @filter IS NOT NULL AND @filter <> N'''' THEN N'' WHERE ''+@filter ELSE N'''' END;

                            IF @dspace IS NOT NULL AND @dspace <> N''''
                            BEGIN
                                SET @idSql = @idSql + N'' ON ''+QUOTENAME(@dspace);
                                IF @ds_type = N''PARTITION_SCHEME'' AND @part_cols_src IS NOT NULL AND @part_cols_src <> N''''
                                BEGIN
                                    SET @idSql = @idSql + N''(''+ @part_cols_src +N'')'';
                                END
                            END
                            SET @idSql = @idSql + N'';'';
                        END

                        -- After constructing @idSql
                        IF @idSql NOT LIKE N''-- ERROR:%'' AND @idSql <> N''''
                        BEGIN
                            -- For debugging, it''s crucial to see the statement being executed:
                            -- You can insert it into a logging table or PRINT it if running manually.
                            -- PRINT @idSql;
                            EXEC (@idSql);
                        END
                        ELSE IF @idSql LIKE N''-- ERROR:%''
                        BEGIN
                            -- Log the error contained in @idSql
                            PRINT @idSql; -- Or log to a table
                        END

                        FETCH NEXT FROM idx INTO @iname,@uniq,@type_desc,@filter,@dspace,@ds_type,
                                                 @key_cols_src,@inc_cols_src,@part_cols_src; -- These are placeholder names from previous version of SP
                                                                                            -- Ensure your cursor actually fetches into these specific variable names.
                    END
                    CLOSE idx; DEALLOCATE idx;

                /* =================================================== *
                 * 6.  Re-create FOREIGN KEY constraints (trusted)      *
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
                    /* =================================================== *
                     * 7.  Re-create CHECK constraints (user-defined)     *
                     * =================================================== */
                    -- First, drop the placeholder CHECK (1=1) constraint if it was planned to be added,
                    -- or ensure this section replaces it.
                    -- Let''s assume we remove the old placeholder:
                    -- (The old placeholder was:
                    --  DECLARE @ckSql nvarchar(max) =
                    --    N''ALTER TABLE '' + QUOTENAME(@Schema) + N''.'' + QUOTENAME(@Stage) +
                    --    N'' WITH NOCHECK ADD CONSTRAINT CK_'' + @Stage + N''_AlwaysTrue CHECK (1 = 1);'';
                    --  EXEC (@ckSql);
                    --  You might want to remove this from the end of your original SP if you add the following)

                    DECLARE @ccName sysname, @ccDefinition nvarchar(max), @ccIsNotTrusted bit, @ccSql nvarchar(max);
        
                    DECLARE cc CURSOR LOCAL FAST_FORWARD FOR
                    SELECT 
                        name AS CheckConstraintName,
                        definition AS Definition,
                        is_not_trusted AS IsNotTrusted -- Important for SWITCH
                    FROM sys.check_constraints
                    WHERE parent_object_id = @BaseObjectId
                      AND type = ''C'' -- User-defined CHECK constraint
                      AND is_disabled = 0; -- Only enabled constraints

                    OPEN cc;
                    FETCH NEXT FROM cc INTO @ccName, @ccDefinition, @ccIsNotTrusted;
                    WHILE @@FETCH_STATUS = 0
                    BEGIN
                        DECLARE @newCcName sysname = LEFT(@ccName + N''_on_'' + @Stage, 128); -- Create a unique name

                        -- Add constraint, initially WITH NOCHECK if source was not trusted or to speed up, then re-check
                        -- For ALTER TABLE SWITCH, constraints must be identical AND trusted.
                        -- It''s generally safer to add WITH CHECK if possible, or ensure they are re-trusted.
                        SET @ccSql = N''ALTER TABLE '' + QUOTENAME(@Schema) + N''.'' + QUOTENAME(@Stage) +
                                     N'' ADD CONSTRAINT '' + QUOTENAME(@newCcName) +
                                     N'' CHECK ('' + @ccDefinition + N'');'';
            
                        -- If the original constraint was not trusted, the new one might also not be initially.
                        -- However, for partition switching, it''s best if they are trusted.
                        -- So, we add it and then explicitly check it to mark it as trusted.
                        -- If the definition itself is bad for existing data (which shouldn''t happen if cloning), 
                        -- the CHECK CONSTRAINT would fail here.
                        IF @ccIsNotTrusted = 1
                        BEGIN
                             -- If source was not trusted, add WITH NOCHECK then try to check it
                             SET @ccSql = N''ALTER TABLE '' + QUOTENAME(@Schema) + N''.'' + QUOTENAME(@Stage) +
                                          N'' WITH NOCHECK ADD CONSTRAINT '' + QUOTENAME(@newCcName) +
                                          N'' CHECK ('' + @ccDefinition + N'');'' +
                                          N'' ALTER TABLE '' + QUOTENAME(@Schema) + N''.'' + QUOTENAME(@Stage) +
                                          N'' CHECK CONSTRAINT '' + QUOTENAME(@newCcName) + N'';''; 
                        END
                        ELSE
                        BEGIN
                             -- If source was trusted, add WITH CHECK (default)
                             SET @ccSql = N''ALTER TABLE '' + QUOTENAME(@Schema) + N''.'' + QUOTENAME(@Stage) +
                                          N'' ADD CONSTRAINT '' + QUOTENAME(@newCcName) +
                                          N'' CHECK ('' + @ccDefinition + N'');'' + 
                                          N'' ALTER TABLE '' + QUOTENAME(@Schema) + N''.'' + QUOTENAME(@Stage) +
                                          N'' CHECK CONSTRAINT '' + QUOTENAME(@newCcName) + N'';''; -- Ensures it''s marked trusted
                        END
            
                        EXEC (@ccSql);
                        FETCH NEXT FROM cc INTO @ccName, @ccDefinition, @ccIsNotTrusted;
                    END
                    CLOSE cc; DEALLOCATE cc;
                END
                ');
                ");

            // ---------------------------------------------------------------------
            // 2)  CALL THE PROC FOR EACH PARTITIONED TABLE 
            // ---------------------------------------------------------------------

            foreach (var table in tableNames)
            {
                migrationBuilder.Sql($@"EXEC dbo.usp_CreateStagingTableForPartitionedTable N'{table}';");
            }

            // Add this temporary check:
            migrationBuilder.Sql(@"
            PRINT N'>>> Checking IDENTITY on AssetSummaryDetailValueIntId_Staging <<<';
            IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID('dbo.AssetSummaryDetailValueIntId_Staging'))
                PRINT N'!!! IDENTITY FOUND on AssetSummaryDetailValueIntId_Staging.Id !!!';
            ELSE
                PRINT N'--- IDENTITY NOT FOUND on AssetSummaryDetailValueIntId_Staging.Id ---';
                
            PRINT N'>>> Checking IDENTITY on AssetDetailValueIntId_Staging <<<';
            IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID('dbo.AssetDetailValueIntId_Staging'))
                PRINT N'!!! IDENTITY FOUND on AssetDetailValueIntId_Staging.Id !!!';
            ELSE
                PRINT N'--- IDENTITY NOT FOUND on AssetDetailValueIntId_Staging.Id ---';
        ");
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
