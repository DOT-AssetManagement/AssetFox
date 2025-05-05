using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Linq;

#nullable disable

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Migrations
{
    /// <inheritdoc />
    public partial class PartitionAllSimulationTables : Migration
    {
        // List all tables being partitioned (ensure schema is correct if not 'dbo')
        private readonly string[] _partitionedTables = {
            "BudgetDetail", "TargetConditionGoalDetail", "DeficientConditionGoalDetail", "CashFlowConsiderationDetail", "Allocation",
            "FundingCalculationOutput", "BudgetToSpend", "FundingCalculationInput",
            "TreatmentConsiderationDetail", "TreatmentSchedulingCollisionDetail", "TreatmentRejectionDetail",
            "TreatmentOptionDetail", "AssetDetailValueIntId", "AssetSummaryDetailValueIntId",
            "AssetDetail", "AssetSummaryDetail", "SimulationYearDetail"
        };

        // Define Schema if not dbo
        private const string SchemaName = "dbo";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Set a very long timeout - adjust as needed based on testing!

            // --- STEP 1: Drop ALL Foreign Keys ---
            // Drop FKs involving the partitioned tables (both incoming and outgoing relative to PKs being changed)
            // Must be done first to allow PKs to be dropped/recreated.
            migrationBuilder.Sql($"PRINT '--- STEP 1: Dropping Foreign Keys ---';");
            // FKs FROM Partitioned Tables
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[BudgetDetail] DROP CONSTRAINT FK_BudgetDetail_SimulationYearDetail_SimulationYearDetailId;");
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[DeficientConditionGoalDetail] DROP CONSTRAINT FK_DeficientConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId;");
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[TargetConditionGoalDetail] DROP CONSTRAINT FK_TargetConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId;");
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[AssetSummaryDetail] DROP CONSTRAINT FK_AssetSummaryDetail_SimulationOutput_SimulationOutputId;");
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[AssetSummaryDetail] DROP CONSTRAINT FK_AssetSummaryDetail_MaintainableAsset_MaintainableAssetId;"); // To non-partitioned table is ok, but might reference PK we drop
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[AssetSummaryDetailValueIntId] DROP CONSTRAINT FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId;");
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[AssetSummaryDetailValueIntId] DROP CONSTRAINT FK_AssetSummaryDetailValueIntId_Attribute_AttributeId;"); // To non-partitioned
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[SimulationYearDetail] DROP CONSTRAINT FK_SimulationYearDetail_SimulationOutput_SimulationOutputId;");
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[AssetDetail] DROP CONSTRAINT FK_AssetDetail_SimulationYearDetail_SimulationYearDetailId;");
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[AssetDetail] DROP CONSTRAINT FK_AssetDetail_MaintainableAsset_MaintainableAssetId;"); // To non-partitioned
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[AssetDetailValueIntId] DROP CONSTRAINT FK_AssetDetailValueIntId_AssetDetail_AssetDetailId;");
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[AssetDetailValueIntId] DROP CONSTRAINT FK_AssetDetailValueIntId_Attribute_AttributeId;"); // To non-partitioned
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[TreatmentOptionDetail] DROP CONSTRAINT FK_TreatmentOptionDetail_AssetDetail_AssetDetailId;");
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[TreatmentRejectionDetail] DROP CONSTRAINT FK_TreatmentRejectionDetail_AssetDetail_AssetDetailId;");
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[TreatmentSchedulingCollisionDetail] DROP CONSTRAINT FK_TreatmentSchedulingCollisionDetail_AssetDetail_AssetDetailId;");
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[TreatmentConsiderationDetail] DROP CONSTRAINT FK_TreatmentConsiderationDetail_AssetDetail_AssetDetailId;");
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[FundingCalculationInput] DROP CONSTRAINT FK_FundingCalculationInput_TreatmentConsiderationDetail_TreatmentConsiderationDetailId;");
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[BudgetToSpend] DROP CONSTRAINT FK_BudgetToSpend_FundingCalculationInput_FundingCalculationInputId;");
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[FundingCalculationOutput] DROP CONSTRAINT FK_FundingCalculationOutput_TreatmentConsiderationDetail_TreatmentConsiderationDetailId;");
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[Allocation] DROP CONSTRAINT FK_Allocation_FundingCalculationOutput_FundingCalculationOutputId;");
            migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[CashFlowConsiderationDetail] DROP CONSTRAINT FK_CashFlowConsiderationDetail_TreatmentConsiderationDetail_TreatmentConsiderationDetailId;");


            // --- STEP 2: Drop ALL existing Primary Keys / Clustered Indexes ---
            migrationBuilder.Sql($"PRINT '--- STEP 2: Dropping Primary Keys / Clustered Indexes ---';");
            foreach (var tableName in _partitionedTables)
            {
                // Assuming PK name follows convention PK_TableName
                migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[{tableName}] DROP CONSTRAINT PK_{tableName};");
                // If CI has different name or exists separately, drop it too: DROP INDEX CI_{tableName} ON ...
            }


            // --- STEP 3: Recreate ALL Primary Keys (as NonClustered) ---
            migrationBuilder.Sql($"PRINT '--- STEP 3: Recreating Primary Keys (NonClustered) ---';");
            foreach (var tableName in _partitionedTables)
            {
                // Determine the original PK column(s) - assuming 'Id' of type Guid or Int
                // Adjust type/name if needed for specific tables (e.g., AssetSummaryDetailValueIntId has INT Id)
                string idColumnType = (tableName.EndsWith("ValueIntId")) ? "[int]" : "[uniqueidentifier]";
                migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[{tableName}] ADD CONSTRAINT PK_{tableName}
                PRIMARY KEY NONCLUSTERED (Id ASC, RunId ASC)
                ON PS_SimulationRun (RunId);
            ");
            }


            // --- STEP 4: Create ALL New Clustered Indexes ON the Partition Scheme ---
            migrationBuilder.Sql($"PRINT '--- STEP 4: Creating New Clustered Indexes (Partitioned) ---';");
            foreach (var tableName in _partitionedTables)
            {
                // Determine the original PK column(s) for the CI definition
                string idColumnName = "Id"; // Assuming 'Id' is always the original PK column
                migrationBuilder.Sql($@"
                CREATE CLUSTERED INDEX CI_{tableName}
                ON [{SchemaName}].[{tableName}] (RunId ASC, {idColumnName} ASC) -- Partition Key First!
                WITH (DROP_EXISTING = OFF, ONLINE = OFF, DATA_COMPRESSION = NONE) -- ONLINE=OFF for CI changes
                ON PS_SimulationRun (RunId); -- Apply Partition Scheme!
            ");
            }


            // --- STEP 5: Recreate Non-Clustered Indexes ON the Partition Scheme (Programmatically) ---
            migrationBuilder.Sql($"PRINT '--- STEP 5: Recreating Non-Clustered Indexes (Partitioned) ---';");

            // Use the dynamic SQL provided by the user to find and recreate NCIs
            migrationBuilder.Sql($@"
                DECLARE @SchemaName SYSNAME = '{SchemaName}'; -- Use the SchemaName variable defined in your migration class
                DECLARE @TableName SYSNAME;
                DECLARE @IndexName SYSNAME;
                DECLARE @IndexID INT;
                DECLARE @IsUnique BIT;
                DECLARE @FilterDefinition NVARCHAR(MAX);
                DECLARE @KeyColumns NVARCHAR(MAX);
                DECLARE @IncludedColumns NVARCHAR(MAX);
                DECLARE @SQL_DROP NVARCHAR(MAX);
                DECLARE @SQL_CREATE NVARCHAR(MAX);
                DECLARE @LiveTableObjectId INT;

                -- Cursor to loop through the specified tables
                DECLARE TableCursor CURSOR LOCAL FAST_FORWARD FOR
                SELECT table_name FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = @SchemaName AND TABLE_NAME IN ({string.Join(", ", _partitionedTables.Select(t => $"'{t}'"))}); -- Use table list defined in your migration class

                OPEN TableCursor;
                FETCH NEXT FROM TableCursor INTO @TableName;

                WHILE @@FETCH_STATUS = 0
                BEGIN
                    PRINT N'Processing Table for NCI Recreation: [' + @SchemaName + N'].[' + @TableName + N']';
                    SET @LiveTableObjectId = OBJECT_ID(@SchemaName + N'.' + @TableName); -- Get Object ID

                    IF @LiveTableObjectId IS NULL
                    BEGIN
                         PRINT N'  WARNING: Could not find Object ID for table [' + @SchemaName + N'].[' + @TableName + N']. Skipping NCI recreation.';
                         GOTO NextTable; -- Skip to the next table in the outer loop
                    END;

                    -- Cursor for Non-Clustered Indexes on the current table
                    DECLARE IndexCursor CURSOR LOCAL FAST_FORWARD FOR
                    SELECT
                        i.name,
                        i.index_id,
                        i.is_unique,
                        i.filter_definition
                    FROM sys.indexes i
                    WHERE i.object_id = @LiveTableObjectId
                      AND i.type = 2 -- Type 2 = Non-Clustered Index
                      AND i.is_primary_key = 0 -- Exclude Non-Clustered Primary Keys (handled in Step 3)
                      AND i.is_unique_constraint = 0; -- Exclude NC Unique Constraints (handled by PK)

                    OPEN IndexCursor;
                    FETCH NEXT FROM IndexCursor INTO @IndexName, @IndexID, @IsUnique, @FilterDefinition;

                    WHILE @@FETCH_STATUS = 0
                    BEGIN
                        PRINT N'  Processing Index: [' + @IndexName + N']';

                        -- Build Key Columns list
                        SELECT @KeyColumns = STUFF(
                            (SELECT N', [' + c.name + N']' + CASE WHEN ic.is_descending_key = 1 THEN N' DESC' ELSE N' ASC' END
                             FROM sys.index_columns ic
                             JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                             WHERE ic.object_id = @LiveTableObjectId
                               AND ic.index_id = @IndexID
                               AND ic.is_included_column = 0 -- Key columns only
                             ORDER BY ic.key_ordinal
                             FOR XML PATH(N''), TYPE).value(N'.[1]', N'NVARCHAR(MAX)'),
                        1, 2, N''); -- Remove leading ', '

                        -- Build Included Columns list
                        SELECT @IncludedColumns = STUFF(
                            (SELECT N', [' + c.name + N']'
                             FROM sys.index_columns ic
                             JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                             WHERE ic.object_id = @LiveTableObjectId
                               AND ic.index_id = @IndexID
                               AND ic.is_included_column = 1 -- Included columns only
                             ORDER BY ic.index_column_id
                             FOR XML PATH(N''), TYPE).value(N'.[1]', N'NVARCHAR(MAX)'),
                        1, 2, N''); -- Remove leading ', '

                        -- Check if KeyColumns were found
                        IF @KeyColumns IS NULL OR LEN(@KeyColumns) = 0
                        BEGIN
                            PRINT N'    WARNING: Could not determine key columns for index [' + @IndexName + N']. Skipping recreation.';
                            GOTO NextIndex; -- Skip to the next index
                        END;

                        -- Adjust Key Columns for UNIQUE Partitioned Indexes
                        DECLARE @FinalKeyColumns NVARCHAR(MAX) = @KeyColumns;
                        IF @IsUnique = 1
                        BEGIN
                            -- Check if partitioning key 'RunId' is already in the key list
                            IF CHARINDEX(N'[RunId]', @KeyColumns) = 0
                            BEGIN
                                -- Append partitioning key RunId (assuming ASC order is acceptable)
                                SET @FinalKeyColumns = @KeyColumns + N', [RunId] ASC'; -- Append RunId to existing keys
                                PRINT N'    NOTE: Appending partitioning key [RunId] ASC to UNIQUE index key for [' + @IndexName + N']';
                            END
                        END

                        -- Generate DROP statement
                        SET @SQL_DROP = N'IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N''' + @IndexName + N''' AND object_id = ' + CAST(@LiveTableObjectId AS VARCHAR(20)) + N') ' +
                                        N'DROP INDEX [' + @IndexName + N'] ON [' + @SchemaName + N'].[' + @TableName + N'];';

                        -- Generate CREATE statement using @FinalKeyColumns
                        SET @SQL_CREATE = N'CREATE ' + CASE WHEN @IsUnique = 1 THEN N'UNIQUE ' ELSE N'' END + N'NONCLUSTERED INDEX [' + @IndexName + N'] ON [' + @SchemaName + N'].[' + @TableName + N'] (' + @FinalKeyColumns + N')'; -- Use potentially modified key list

                        IF @IncludedColumns IS NOT NULL AND LEN(@IncludedColumns) > 0
                        BEGIN
                            SET @SQL_CREATE = @SQL_CREATE + N' INCLUDE (' + @IncludedColumns + N')';
                        END

                        IF @FilterDefinition IS NOT NULL AND LEN(@FilterDefinition) > 0
                        BEGIN
                            SET @SQL_CREATE = @SQL_CREATE + N' WHERE ' + @FilterDefinition;
                        END

                        -- *** CRITICAL: Add ON Partition Scheme ***
                        SET @SQL_CREATE = @SQL_CREATE + N' ON PS_SimulationRun (RunId);'; -- Append partitioning clause

                        -- Execute DROP and CREATE
                        BEGIN TRY
                            PRINT N'    Executing DROP: ' + @SQL_DROP;
                            EXEC (@SQL_DROP);
                            PRINT N'    Executing CREATE: ' + @SQL_CREATE;
                            EXEC (@SQL_CREATE);
                            PRINT N'    Index [' + @IndexName + N'] recreated successfully on partition scheme.';
                        END TRY
                        BEGIN CATCH
                             PRINT N'    ERROR recreating index [' + @IndexName + N']: ' + ERROR_MESSAGE();
                             -- *** ADDED THROW TO MAKE FAILURE EXPLICIT ***
                             THROW;
                        END CATCH;

                        NextIndex: -- Label for skipping index
                        FETCH NEXT FROM IndexCursor INTO @IndexName, @IndexID, @IsUnique, @FilterDefinition;
                    END; -- End inner WHILE loop

                    CLOSE IndexCursor;
                    DEALLOCATE IndexCursor;

                    NextTable: -- Label for skipping table
                    FETCH NEXT FROM TableCursor INTO @TableName;
                END; -- End outer WHILE loop

                CLOSE TableCursor;
                DEALLOCATE TableCursor;

                PRINT N'--- Non-Clustered Index Recreation Complete ---';
            ");


            // --- STEP 6: Recreate ALL Foreign Keys ---
            migrationBuilder.Sql($"PRINT '--- STEP 6: Recreating Foreign Keys WITH NOCHECK ---';");

            // --- Recreate Foreign Keys ---

            // AssetSummaryDetail FKs
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[AssetSummaryDetail] WITH NOCHECK ADD CONSTRAINT FK_AssetSummaryDetail_MaintainableAsset_MaintainableAssetId
                FOREIGN KEY([MaintainableAssetId]) REFERENCES [{SchemaName}].[MaintainableAsset] ([Id]);
            ");
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[AssetSummaryDetail] WITH NOCHECK ADD CONSTRAINT FK_AssetSummaryDetail_SimulationOutput_SimulationOutputId
                FOREIGN KEY([SimulationOutputId]) REFERENCES [{SchemaName}].[SimulationOutput] ([Id]) ON DELETE CASCADE;
            ");

            // AssetSummaryDetailValueIntId FKs
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[AssetSummaryDetailValueIntId] WITH NOCHECK ADD CONSTRAINT FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId
                FOREIGN KEY([AssetSummaryDetailId], [RunId]) REFERENCES [{SchemaName}].[AssetSummaryDetail] ([Id], [RunId])
                ON DELETE CASCADE;
            ");
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[AssetSummaryDetailValueIntId] WITH NOCHECK ADD CONSTRAINT FK_AssetSummaryDetailValueIntId_Attribute_AttributeId
                FOREIGN KEY([AttributeId]) REFERENCES [{SchemaName}].[Attribute] ([Id]);
            ");

            // SimulationYearDetail FKs
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[SimulationYearDetail] WITH NOCHECK ADD CONSTRAINT FK_SimulationYearDetail_SimulationOutput_SimulationOutputId
                FOREIGN KEY([SimulationOutputId]) REFERENCES [{SchemaName}].[SimulationOutput] ([Id]) ON DELETE CASCADE;
            ");

            // AssetDetail FKs
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[AssetDetail] WITH NOCHECK ADD CONSTRAINT FK_AssetDetail_MaintainableAsset_MaintainableAssetId
                FOREIGN KEY([MaintainableAssetId]) REFERENCES [{SchemaName}].[MaintainableAsset] ([Id]);
            ");
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[AssetDetail] WITH NOCHECK ADD CONSTRAINT FK_AssetDetail_SimulationYearDetail_SimulationYearDetailId
                FOREIGN KEY([SimulationYearDetailId], [RunId]) REFERENCES [{SchemaName}].[SimulationYearDetail] ([Id], [RunId])
                ON DELETE CASCADE;
            ");

            // AssetDetailValueIntId FKs
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[AssetDetailValueIntId] WITH NOCHECK ADD CONSTRAINT FK_AssetDetailValueIntId_AssetDetail_AssetDetailId
                FOREIGN KEY([AssetDetailId], [RunId]) REFERENCES [{SchemaName}].[AssetDetail] ([Id], [RunId])
                ON DELETE CASCADE;
            ");
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[AssetDetailValueIntId] WITH NOCHECK ADD CONSTRAINT FK_AssetDetailValueIntId_Attribute_AttributeId
                FOREIGN KEY([AttributeId]) REFERENCES [{SchemaName}].[Attribute] ([Id]);
            ");

            // TreatmentOptionDetail FKs
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[TreatmentOptionDetail] WITH NOCHECK ADD CONSTRAINT FK_TreatmentOptionDetail_AssetDetail_AssetDetailId
                FOREIGN KEY([AssetDetailId], [RunId]) REFERENCES [{SchemaName}].[AssetDetail] ([Id], [RunId])
                ON DELETE CASCADE;
            ");

            // TreatmentRejectionDetail FKs
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[TreatmentRejectionDetail] WITH NOCHECK ADD CONSTRAINT FK_TreatmentRejectionDetail_AssetDetail_AssetDetailId
                FOREIGN KEY([AssetDetailId], [RunId]) REFERENCES [{SchemaName}].[AssetDetail] ([Id], [RunId])
                ON DELETE CASCADE;
            ");

            // TreatmentSchedulingCollisionDetail FKs
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[TreatmentSchedulingCollisionDetail] WITH NOCHECK ADD CONSTRAINT FK_TreatmentSchedulingCollisionDetail_AssetDetail_AssetDetailId
                FOREIGN KEY([AssetDetailId], [RunId]) REFERENCES [{SchemaName}].[AssetDetail] ([Id], [RunId])
                ON DELETE CASCADE;
            ");

            // TreatmentConsiderationDetail FKs
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[TreatmentConsiderationDetail] WITH NOCHECK ADD CONSTRAINT FK_TreatmentConsiderationDetail_AssetDetail_AssetDetailId
                FOREIGN KEY([AssetDetailId], [RunId]) REFERENCES [{SchemaName}].[AssetDetail] ([Id], [RunId])
                ON DELETE CASCADE;
            ");

            // FundingCalculationInput FKs
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[FundingCalculationInput] WITH NOCHECK ADD CONSTRAINT FK_FundingCalculationInput_TreatmentConsiderationDetail_TreatmentConsiderationDetailId
                FOREIGN KEY([TreatmentConsiderationDetailId], [RunId]) REFERENCES [{SchemaName}].[TreatmentConsiderationDetail] ([Id], [RunId])
                ON DELETE CASCADE;
            ");

            // BudgetToSpend FKs
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[BudgetToSpend] WITH NOCHECK ADD CONSTRAINT FK_BudgetToSpend_FundingCalculationInput_FundingCalculationInputId
                FOREIGN KEY([FundingCalculationInputId], [RunId]) REFERENCES [{SchemaName}].[FundingCalculationInput] ([Id], [RunId])
                ON DELETE CASCADE;
            ");

            // FundingCalculationOutput FKs
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[FundingCalculationOutput] WITH NOCHECK ADD CONSTRAINT FK_FundingCalculationOutput_TreatmentConsiderationDetail_TreatmentConsiderationDetailId
                FOREIGN KEY([TreatmentConsiderationDetailId], [RunId]) REFERENCES [{SchemaName}].[TreatmentConsiderationDetail] ([Id], [RunId])
                ON DELETE CASCADE;
            ");

            // Allocation FKs
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[Allocation] WITH NOCHECK ADD CONSTRAINT FK_Allocation_FundingCalculationOutput_FundingCalculationOutputId
                FOREIGN KEY([FundingCalculationOutputId], [RunId]) REFERENCES [{SchemaName}].[FundingCalculationOutput] ([Id], [RunId])
                ON DELETE CASCADE;
            ");

            // CashFlowConsiderationDetail FKs
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[CashFlowConsiderationDetail] WITH NOCHECK ADD CONSTRAINT FK_CashFlowConsiderationDetail_TreatmentConsiderationDetail_TreatmentConsiderationDetailId
                FOREIGN KEY([TreatmentConsiderationDetailId], [RunId]) REFERENCES [{SchemaName}].[TreatmentConsiderationDetail] ([Id], [RunId])
                ON DELETE CASCADE;
            ");

            // DeficientConditionGoalDetail FKs
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[DeficientConditionGoalDetail] WITH NOCHECK ADD CONSTRAINT FK_DeficientConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId
                FOREIGN KEY([SimulationYearDetailId], [RunId]) REFERENCES [{SchemaName}].[SimulationYearDetail] ([Id], [RunId])
                ON DELETE CASCADE;
            ");

            // TargetConditionGoalDetail FKs
            migrationBuilder.Sql($@"
                ALTER TABLE [{SchemaName}].[TargetConditionGoalDetail] WITH NOCHECK ADD CONSTRAINT FK_TargetConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId
                FOREIGN KEY([SimulationYearDetailId], [RunId]) REFERENCES [{SchemaName}].[SimulationYearDetail] ([Id], [RunId])
                ON DELETE CASCADE;
            ");

            // Optional but recommended: Run CHECK CONSTRAINT after all DDL is done.
            // Could be a separate SQL script run manually after migration.
            foreach (var tableName in _partitionedTables) {
                migrationBuilder.Sql($"ALTER TABLE [{SchemaName}].[{tableName}] WITH CHECK CHECK CONSTRAINT ALL;");
             }
            migrationBuilder.Sql($"PRINT '--- Partitioning Migration Complete ---';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ** Recommended: Mark as Irreversible **
            throw new Exception("Reversing the partitioning of multiple simulation tables requires manual intervention or database restore. This automatic rollback is not supported.");

            // ** If attempting reversal (HIGHLY COMPLEX/RISKY - NOT RECOMMENDED) **
            // You would need to script the reverse of ALL steps in the Up() method,
            // in the reverse order, rebuilding tables back onto the PRIMARY filegroup.
            // migrationBuilder.SetTimeout(3600 * 4); // Needs long timeout too
            // migrationBuilder.Sql("PRINT '--- STARTING DOWN MIGRATION (Reversing Partitioning - COMPLEX/RISKY) ---';");
            // 1. Drop FKs added in Up() Step 6
            // 2. Drop NCIs added in Up() Step 5
            // 3. Drop CIs added in Up() Step 4
            // 4. Drop PKs added in Up() Step 3
            // 5. Recreate original PKs/CIs (rebuilds tables onto PRIMARY)
            // 6. Recreate original FKs
            // migrationBuilder.Sql("PRINT '--- DOWN MIGRATION COMPLETE (Partitioning Reversed) ---';");
        }
    }
}
