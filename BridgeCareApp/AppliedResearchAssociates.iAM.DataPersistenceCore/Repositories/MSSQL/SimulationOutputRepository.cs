using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.Common;
using AppliedResearchAssociates.iAM.Common.Logging;
using AppliedResearchAssociates.iAM.Common.PerformanceMeasurement;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Enums;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Extensions;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DTOs;
using EFCore.BulkExtensions;
using Humanizer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using static System.Runtime.InteropServices.JavaScript.JSType;
using FundingCalculationInput = AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities.FundingCalculationInput;
using FundingCalculationOutput = AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities.FundingCalculationOutput;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL
{
    public class SimulationOutputRepository : ISimulationOutputRepository
    {
        private const bool ShouldHackSaveOutputToFile = false;
        private const bool ShouldHackSaveTimingsToFile = false;
        public const string SimulationOutputLoadKey = "SimulationOutputSqlBatches";
        public const string AssetLoadBatchSizeOverrideKey = "AssetDetailBatchSizeOverrideForValueLoad";
        private readonly UnitOfDataPersistenceWork _unitOfWork;
        public const int AssetLoadBatchSize = 2000;
        public const int AssetDetailSaveBatchSize = 100000;
        public const string AssetDetailSaveOverrideBatchSizeKey = "AssetDetailBatchSizeOverrideForValueSave";
        private readonly ILog _log;

        public SimulationOutputRepository(UnitOfDataPersistenceWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public void CreateSimulationOutputViaRelational(Guid simulationId, SimulationOutput simulationOutput,
            IWorkQueueLog loggerForUserInfo = null, ILog loggerForTechnicalInfo = null, CancellationToken? cancellationToken = null)
        {
            var stopwatch = Stopwatch.StartNew();

            loggerForTechnicalInfo ??= new DoNotLog();
            loggerForUserInfo ??= new DoNothingWorkQueueLog();
            loggerForUserInfo.UpdateWorkQueueStatus("Preparing to save to database");
            var _log = new DoLog();

            if (ShouldHackSaveOutputToFile)
            {
#pragma warning disable CS0162 // Unreachable code detected
                HackSaveOutputToFile(simulationOutput);
#pragma warning restore CS0162 // Unreachable code detected
            }

            var saveMemos = EventMemoModelLists.GetFreshInstance("Save");
            var simulationMemos = EventMemoModelLists.GetInstance("Simulation");
            _ = simulationMemos.Mark("Starting save");
            var startMemo = saveMemos.MarkInformation("Starting save", loggerForTechnicalInfo);

            stopwatch.Stop();
            _log.Information($"Starting sim save process. {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Start();

            if (!_unitOfWork.Context.Simulation.Any(_ => _.Id == simulationId))
            {
                throw new RowNotInTableException("No simulation found for given scenario.");
            }

            var simulationEntity = _unitOfWork.Context.Simulation.AsNoTracking()
                .Single(_ => _.Id == simulationId);

            if (simulationOutput == null)
            {
                throw new InvalidOperationException($"No results found for simulation {simulationEntity.Name}. Please ensure that the simulation analysis has been run.");
            }

            var allAttributes = _unitOfWork.AttributeRepo.GetAttributes();
            var attributeIdLookup = new Dictionary<string, Guid>();
            foreach (var attribute in allAttributes)
            {
                attributeIdLookup[attribute.Name] = attribute.Id;
            }

            int currentSimulationRunId = -1; // Initialize

            try
            {
                if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
                {
                    _unitOfWork.Rollback();
                    return;
                }

                // --- Step 1: Handle Existing Data & Get Old Run ID (if any) ---
                string[] order =
                {
                    "dbo.AssetSummaryDetail",
                    "dbo.AssetSummaryDetailValueIntId",
                    "dbo.SimulationYearDetail",
                    "dbo.AssetDetail",
                    "dbo.AssetDetailValueIntId",
                    "dbo.TreatmentOptionDetail",
                    "dbo.TreatmentRejectionDetail",
                    "dbo.TreatmentSchedulingCollisionDetail",
                    "dbo.TreatmentConsiderationDetail",
                    "dbo.FundingCalculationInput",
                    "dbo.FundingCalculationOutput",
                    "dbo.BudgetToSpend",
                    "dbo.Allocation",
                    "dbo.CashFlowConsiderationDetail",
                    "dbo.BudgetDetail",
                    "dbo.DeficientConditionGoalDetail",
                    "dbo.TargetConditionGoalDetail"
                };

                ClearStagingTables(order);

                int? oldSimulationRunId = null;
                var existingOutputEntity = _unitOfWork.Context.SimulationOutput
                    .SingleOrDefault(so => so.SimulationId == simulationId); // Find based on logical ID

                if (existingOutputEntity != null)
                {
                    oldSimulationRunId = existingOutputEntity.RunId;
                    _log.Information($"Found existing data for SimulationId {simulationId} with SimulationRunId {oldSimulationRunId}. Preparing to remove.");

                    // --- Step 2: Delete Old Data using SWITCH OUT ---
                    if (oldSimulationRunId.HasValue)
                    {
                        DeleteSimulationDataByRunId(oldSimulationRunId.Value, _log);
                        _log.Information($"Switched out data for old SimulationRunId {oldSimulationRunId}.");
                    }
                }

                

                foreach ( var orderItem in order)
                {
                    var tableName = orderItem + "_Staging";

                    string disableSql = $@"
                        DECLARE @t sysname  = N'{tableName}';
                        DECLARE @s nvarchar(max) = N'';

                        SELECT @s = @s +
                               N'ALTER INDEX ' + QUOTENAME(i.name) +
                               N' ON ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(o.name) +
                               N' DISABLE;' + CHAR(10)
                        FROM sys.indexes i
                        JOIN sys.objects o ON o.object_id = i.object_id
                        WHERE  o.object_id = OBJECT_ID(@t)
                          AND  i.type_desc = 'NONCLUSTERED'          -- only the NCIs
                          AND  i.is_disabled = 0;                    -- skip if already disabled

                        EXEC (@s);                                   -- run all ALTERs in one batch
                    ";

                    _unitOfWork.Context.Database.ExecuteSqlRaw(disableSql);

                    var noCheckSql = $"ALTER TABLE {tableName} NOCHECK CONSTRAINT ALL;";
                    _unitOfWork.Context.Database.ExecuteSqlRaw(noCheckSql);
                }

                // --- Step 3: Insert New SimulationOutput Record & Get NEW SimulationRunId ---
                loggerForUserInfo.UpdateWorkQueueStatus("Saving simulation header...");
                var simulationOutputEntity = SimulationOutputMapper.ToEntityWithoutAssetsOrYearDetails(simulationOutput, simulationId, attributeIdLookup); // Pass the logical simulationId

                // make sure there IS a free partition BEFORE we insert root row
                _unitOfWork.Context.Database.ExecuteSqlRaw("EXEC dbo.usp_EnsureNextRunIdHasPartition;");

                _unitOfWork.Context.SimulationOutput.Add(simulationOutputEntity);
                _unitOfWork.Context.SaveChanges(); // Save to generate the IDENTITY value

                currentSimulationRunId = simulationOutputEntity.RunId; // RETRIEVE THE GENERATED ID
                if (currentSimulationRunId <= 0) // Basic validation
                {
                    throw new InvalidOperationException("Failed to retrieve a valid SimulationRunId after insert.");
                }
                _log.Information($"Created new SimulationOutput record for SimulationId {simulationId} with RunId {currentSimulationRunId}.");

                PrepareStagingTables(order);

                var configuredBatchSize = GetConfiguredBatchSize(_unitOfWork.Config, AssetDetailSaveOverrideBatchSizeKey);
                var batchSize = configuredBatchSize ?? AssetDetailSaveBatchSize;

                var assetSummaries = simulationOutput.InitialAssetSummaries;
                _ = saveMemos.Mark("assetSummaries");

                var family = AssetSummaryDetailMapper.ToEntityLists(assetSummaries, simulationOutputEntity.Id, attributeIdLookup, currentSimulationRunId);

                
                _unitOfWork.Context.BulkInsert(family.AssetSummaryDetails, createConfig("AssetSummaryDetail_Staging", batchSize));
                //_unitOfWork.Context.SaveChanges();
                _ = simulationMemos.Mark("assetSummaryDetails");

                int tempValueIdCounter = 1;
                foreach (var valueEntity in family.AssetSummaryDetailValues)
                {
                    // Assign a temporary, batch-unique ID
                    valueEntity.Id = tempValueIdCounter++;
                }
                _log.Debug($"Assigned temporary IDs to {family.AssetSummaryDetailValues.Count} AssetSummaryDetailValueIntId entities.");

                
                _unitOfWork.Context.BulkInsert(family.AssetSummaryDetailValues, createConfig("AssetSummaryDetailValueIntId_Staging", batchSize));
                //_unitOfWork.Context.SaveChanges();
                _ = saveMemos.Mark("assetSummaryDetailValues");

                stopwatch.Stop();
                _log.Information($"Finished Initial Asset Summary save. Beginning Years save. {stopwatch.ElapsedMilliseconds}ms");
                stopwatch.Start();

                //reset for year detail values
                tempValueIdCounter = 1;

                foreach (var year in simulationOutput.Years)
                {
                    loggerForUserInfo.UpdateWorkQueueStatus($"Saving year {year.Year}");
                    if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
                    {
                        _unitOfWork.Rollback();
                        return;
                    }
                    stopwatch.Stop();
                    _log.Information($"Starting save for {year.Year}. {stopwatch.ElapsedMilliseconds}ms");
                    stopwatch.Start();

                    var yearMemo = saveMemos.MarkInformation($"Y{year.Year}", loggerForTechnicalInfo);
                    var yearDetail = SimulationYearDetailMapper.ToEntityWithoutAssets(year, simulationOutputEntity.Id, attributeIdLookup, currentSimulationRunId);

                    _unitOfWork.Context.BulkInsert(new List<SimulationYearDetailEntity> { yearDetail }, createConfig("SimulationYearDetail_Staging", batchSize));

                    _unitOfWork.Context.BulkInsert(yearDetail.Budgets, createConfig("BudgetDetail_Staging", batchSize));
                    _ = saveMemos.Mark($" {yearDetail.Budgets.Count} budgets");

                    _unitOfWork.Context.BulkInsert(yearDetail.DeficientConditionGoals, createConfig("DeficientConditionGoalDetail_Staging", batchSize));
                    _ = saveMemos.Mark($" {yearDetail.DeficientConditionGoals.Count} deficientConditionGoals");

                    _unitOfWork.Context.BulkInsert(yearDetail.TargetConditionGoals, createConfig("TargetConditionGoalDetail_Staging", batchSize));
                    _ = saveMemos.Mark($" {yearDetail.TargetConditionGoals.Count} targetConditionGoals");


                    var assets = year.Assets;
                    var assetFamily = AssetDetailMapper.ToEntityFamily(assets, yearDetail.Id, attributeIdLookup, currentSimulationRunId);
                    
                    _unitOfWork.Context.BulkInsert(assetFamily.AssetDetails, createConfig("AssetDetail_Staging", batchSize));
                    _ = saveMemos.Mark($" {assetFamily.AssetDetails.Count} assetDetails");

                    foreach (var valueEntity in assetFamily.AssetDetailValues)
                    {
                        // Assign a temporary, batch-unique ID
                        valueEntity.Id = tempValueIdCounter++;
                    }
                    _log.Debug($"Assigned temporary IDs to {family.AssetSummaryDetailValues.Count} AssetDetailValueIntId entities.");


                    _unitOfWork.Context.BulkInsert(assetFamily.AssetDetailValues, createConfig("AssetDetailValueIntId_Staging", batchSize));
                    _ =saveMemos.Mark($" {assetFamily.AssetDetailValues.Count} assetDetailValues batchSize: {batchSize}");
                    
                    _unitOfWork.Context.BulkInsert(assetFamily.TreatmentOptions, createConfig("TreatmentOptionDetail_Staging", batchSize));
                    _ = saveMemos.Mark($" {assetFamily.TreatmentOptions.Count} treatmentOptions");
                    
                    _unitOfWork.Context.BulkInsert(assetFamily.TreatmentRejections, createConfig("TreatmentRejectionDetail_Staging", batchSize));
                    _ = saveMemos.Mark($" {assetFamily.TreatmentRejections.Count} treatmentRejections");
                    
                    _unitOfWork.Context.BulkInsert(assetFamily.TreatmentSchedulingCollisions, createConfig("TreatmentSchedulingCollisionDetail_Staging", batchSize));
                    _ = saveMemos.Mark($" {assetFamily.TreatmentSchedulingCollisions.Count} treatmentSchedulingCollisions");
                    
                    _unitOfWork.Context.BulkInsert(assetFamily.TreatmentConsiderations, createConfig("TreatmentConsiderationDetail_Staging", batchSize));
                    _ = saveMemos.Mark($" {assetFamily.TreatmentConsiderations.Count} treatmentConsiderations");
                    
                    _unitOfWork.Context.BulkInsert(assetFamily.FundingCalculationInputs, createConfig("FundingCalculationInput_Staging", batchSize));
                    _ = saveMemos.Mark($" {assetFamily.FundingCalculationInputs.Count} fundingCalculationInputs");
                    
                    _unitOfWork.Context.BulkInsert(assetFamily.CurrentBudgetsToSpend, createConfig("BudgetToSpend_Staging", batchSize));
                    _ = saveMemos.Mark($" {assetFamily.CurrentBudgetsToSpend.Count} currentBudgetsToSpend");
                    
                    _unitOfWork.Context.BulkInsert(assetFamily.FundingCalculationOutputs, createConfig("FundingCalculationOutput_Staging", batchSize));
                    _ = saveMemos.Mark($" {assetFamily.FundingCalculationOutputs.Count} fundingCalculationOutputs");
                    
                    _unitOfWork.Context.BulkInsert(assetFamily.AllocationMatrix, createConfig("Allocation_Staging", batchSize));
                    _ = saveMemos.Mark($" {assetFamily.AllocationMatrix.Count} allocationMatrix");
                    
                    _unitOfWork.Context.BulkInsert(assetFamily.CashFlowConsiderations, createConfig("CashFlowConsiderationDetail_Staging", batchSize));
                    _ = saveMemos.Mark($" {assetFamily.CashFlowConsiderations.Count} cashFlowConsiderations");

                    stopwatch.Stop();
                    _log.Information($"Finished Saving {year.Year}. {stopwatch.ElapsedMilliseconds}ms");
                    stopwatch.Start();

                    _unitOfWork.Context.ChangeTracker.Clear();
                    _ = saveMemos.Mark(" Cleared ChangeTracker");
                }

                stopwatch.Stop();
                _log.Information($"Starting switch in from staging. {stopwatch.ElapsedMilliseconds}ms");
                stopwatch.Start();

                _unitOfWork.BeginTransaction(); // Start a single transaction for the entire copy process
                //var originalTimeout = _unitOfWork.Context.Database.GetCommandTimeout();
                //_unitOfWork.Context.Database.SetCommandTimeout(300); // e.g., 30 minutes timeout for the copy operations

                try
                {
                    foreach (var liveTable in order)
                    {
                        string stagingTable = liveTable + "_Staging";

                        stopwatch.Stop();
                        _log.Information($"Rebuilding constraints on {stagingTable}. {stopwatch.ElapsedMilliseconds}ms");
                        stopwatch.Start();

                        var enableSql = $"ALTER INDEX ALL ON {stagingTable} REBUILD PARTITION = {currentSimulationRunId};";
                        _unitOfWork.Context.Database.ExecuteSqlRaw(enableSql);

                        var checkSql = $"ALTER TABLE {stagingTable} WITH CHECK CHECK CONSTRAINT ALL;";
                        _unitOfWork.Context.Database.ExecuteSqlRaw(checkSql);

                        SwitchInPartition(stagingTable, liveTable, currentSimulationRunId);
                        _log.Information($"Switched partition for {liveTable}");
                        // Optional: Add cancellation check here too
                    }

                    _unitOfWork.Commit();
                    _log.Information($"Partition Switch In committed successfully for RunId {currentSimulationRunId}.");
                }
                catch (Exception ex)
                {
                    _log.Error($"Error during partition switch for RunId {currentSimulationRunId}. Rolling back.");
                    _unitOfWork.Rollback();
                    throw; // Re-throw
                }
                finally
                {
                    //_unitOfWork.Context.Database.SetCommandTimeout(originalTimeout);
                }

                stopwatch.Stop();
                _log.Information($"Partition switching complete. Time: {stopwatch.ElapsedMilliseconds}ms.");


                _ = saveMemos.MarkInformation("Save complete", loggerForTechnicalInfo);
                _ = simulationMemos.Mark("Save complete");

                if (ShouldHackSaveTimingsToFile)
                {
                    var timingsOutputFilename = "SaveTimings.txt";
                    var simulationOutputFilename = "SimulationTimings.txt";
                    WriteTimingsToFile(saveMemos, timingsOutputFilename);
                    WriteTimingsToFile(simulationMemos, simulationOutputFilename);
                }
                loggerForUserInfo.UpdateWorkQueueStatus(SimulationUserMessages.SimulationOutputSavedToDatabase);
            }
            catch (Exception ex)
            {
                var error = saveMemos.Mark($"Save failed with exception {ex.Message}");
                loggerForTechnicalInfo.Error(error);
                //_unitOfWork.Rollback();
                throw;
            }

            BulkConfig createConfig(string heapTable, int batchSize)
            {
                var cfg = new BulkConfig()
                {
                    BatchSize = batchSize,
                    PreserveInsertOrder = false,
                    SqlBulkCopyOptions = EFCore.BulkExtensions.SqlBulkCopyOptions.TableLock,
                    BulkCopyTimeout = 1800,
                    CustomDestinationTableName = $"dbo.{ heapTable }",
                    CalculateStats = true
                };

                return cfg;
            }

            void ClearStagingTables(string[] tableList)
            {
                foreach (var table in tableList)
                {
                    var stagingTableName = table + "_Staging";
                    string truncateSql = $"TRUNCATE TABLE {stagingTableName};";
                    try
                    {
                        // Use ExecuteSqlRaw for DDL
                        _unitOfWork.Context.Database.ExecuteSqlRaw(truncateSql);
                        _log.Debug($"Truncated staging table {stagingTableName}");
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Error truncating staging table {stagingTableName}. SQL attempted: {truncateSql}");
                        throw; // Re-throw truncation errors, as we can't proceed
                    }
                }
            }
            void PrepareStagingTables(string[] tableList)
            {
                foreach (var table in tableList)
                {
                    var stagingTableName = table + "_Staging";

                    // Drop existing check constraint (assuming a fixed name)
                    var cleanTable = table.StartsWith("dbo.") ? table.Substring("dbo.".Length) : table;
                    string constraintName = $"CK_{cleanTable}_Partition"; // Need consistent naming
                    string dropConstraintSql = $"ALTER TABLE {stagingTableName} DROP CONSTRAINT {constraintName};";
                    try
                    {
                        _unitOfWork.Context.Database.ExecuteSqlRaw(dropConstraintSql);
                        _log.Debug($"Dropped existing constraint {constraintName} on {stagingTableName}.");
                    }
                    // SQL Server error numbers for "constraint does not exist"
                    catch (SqlException ex) when (ex.Number == 3727 || ex.Number == 3728)
                    {
                        _log.Debug($"Constraint {constraintName} not found on {stagingTableName} (or permission issue dropping), continuing assuming it's absent.");
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Unexpected error dropping constraint {constraintName} on {stagingTableName}. SQL attempted: {dropConstraintSql} | Error Message: {ex}");
                        // Decide if this is critical - often it isn't if the goal is just to add the new one.
                        // If it IS critical, uncomment the next line:
                        // throw;
                    }

                    // Add new check constraint for the current runId
                    string addConstraintSql = $"ALTER TABLE {stagingTableName} ADD CONSTRAINT {constraintName} CHECK (RunId = {currentSimulationRunId});";
                    try
                    {
                        _unitOfWork.Context.Database.ExecuteSqlRaw(addConstraintSql);
                        _log.Debug($"Added constraint {constraintName} CHECK (RunId = {currentSimulationRunId}) on {stagingTableName}.");
                    }
                    catch (Exception ex)
                    {
                        // Failure here IS critical, as the staging table won't be valid for switching
                        _log.Error($"FATAL: Error adding constraint {constraintName} with RunId {currentSimulationRunId} on {stagingTableName}. SQL attempted: {addConstraintSql}");
                        throw new InvalidOperationException($"Failed to add partition check constraint to {stagingTableName} for RunId {currentSimulationRunId}. Message: {ex}");
                    }
                    _log.Debug($"Prepared staging table {stagingTableName} for RunId {currentSimulationRunId}");
                }
            }
        }

        private void SwitchInPartition(string stagingTableName, string liveTableName, int simulationRunId, string partitionFunctionName = "PF_SimulationRun", ILog logger = null)
        {
            // Use provided logger or a null logger that does nothing
            var _log = logger ?? new DoNotLog();

            if (_unitOfWork.Context.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Must be called inside an open transaction.");

            // split and quote the names once
            string Quote(string twoPart)
            {
                var parts = twoPart.Split('.');
                return $"[{parts[0]}].[{parts[1]}]";
            }

            string src = Quote(stagingTableName);
            string dest = Quote(liveTableName);

            _unitOfWork.Context.Database.ExecuteSqlRaw(
                $"ALTER TABLE {stagingTableName} WITH CHECK CHECK CONSTRAINT ALL;");

            string sql = $@"
                DECLARE @p int = $PARTITION.{partitionFunctionName}({simulationRunId});
                IF @p IS NULL
                    THROW 50103, N'RunId {simulationRunId} not covered by partition function.', 1;

                DECLARE @cmd nvarchar(max) = 
                    N'ALTER TABLE {src} SWITCH PARTITION ' + CAST(@p AS varchar(10)) +
                    N' TO {dest} PARTITION ' + CAST(@p AS varchar(10)) + N';';

                EXEC (@cmd);";

            _log.Debug(sql);
            _unitOfWork.Context.Database.ExecuteSqlRaw(sql);
        }


        private void DeleteSimulationDataByRunId(int simulationRunId, ILog logger = null)
        {
            var _log = logger ?? new DoNotLog(); // Use provided logger or a null logger

            var outputId = _unitOfWork.Context.SimulationOutput.Where(_ => _.RunId == simulationRunId).Select(_ => _.Id).FirstOrDefault();

            _unitOfWork.BeginTransaction();

            if (_unitOfWork.Context.SimulationOutputJson.Any(_ => _.SimulationOutputId == outputId))
            {
                _unitOfWork.Context.DeleteAll<SimulationOutputJsonEntity>(_ => _.SimulationOutputId == outputId);
            }

            // List all tables that are partitioned by SimulationRunId
            // Order: Children before Parents (important if FKs existed, though SWITCH bypasses checks)
            string[] partitionedTables = {
                // Children of AssetDetail / FundingCalculation* / TreatmentConsideration*
                "dbo.BudgetDetail",
                "dbo.AssetDetailValueIntId",
                "dbo.AssetSummaryDetailValueIntId", // Child of AssetSummaryDetail
                "dbo.TreatmentOptionDetail",
                "dbo.TreatmentRejectionDetail",
                "dbo.TreatmentSchedulingCollisionDetail",
                "dbo.BudgetToSpend",                // Child of FundingCalculationInput
                "dbo.Allocation",                   // Child of FundingCalculationOutput
                "dbo.CashFlowConsiderationDetail",  // Child of TreatmentConsiderationDetail
                "dbo.TargetConditionGoalDetail",
                "dbo.DeficientConditionGoalDetail",
                // Parents
                "dbo.FundingCalculationInput",      // Parent of BudgetToSpend
                "dbo.FundingCalculationOutput",     // Parent of Allocation
                "dbo.TreatmentConsiderationDetail", // Parent of Funding*, CashFlow*
                "dbo.AssetDetail",                  // Parent of ValueIntId, Treatment*
                "dbo.AssetSummaryDetail",           // Parent of ValueIntId
                "dbo.SimulationYearDetail",         // Parent of AssetDetail
            };

            _log.Information($"Starting delete (SWITCH OUT) process for SimulationRunId {simulationRunId}");

            try
            {
                if (_unitOfWork.Context.AssetSummaryDetail.Where(_ => _.RunId == simulationRunId).Any())
                {
                    foreach (var liveTable in partitionedTables) // Loop through (already reversed)
                    {
                        // 1. Disable every FK that points to this liveTable
                        _unitOfWork.Context.Database.ExecuteSqlRaw(@"
                            DECLARE @sql nvarchar(max) = N'';
                            SELECT @sql = @sql + 
                                  N'ALTER TABLE ' 
                                + QUOTENAME(OBJECT_SCHEMA_NAME(fkc.parent_object_id))
                                + N'.' + QUOTENAME(OBJECT_NAME(fkc.parent_object_id))
                                + N' NOCHECK CONSTRAINT ' + QUOTENAME(fk.name) + N';'
                            FROM   sys.foreign_keys fk
                            JOIN   sys.foreign_key_columns fkc
                                     ON fkc.constraint_object_id = fk.object_id
                            WHERE  fk.referenced_object_id = OBJECT_ID({0});
                            EXEC (@sql);", liveTable);

                        // 2. Switch-out the partition
                        string sql = $@"
                            EXEC dbo.usp_PurgePartitionViaSwitchOut
                                @SourceTable = '{Quote(liveTable)}',
                                @PartitionValue = {simulationRunId}
                        ";

                        _unitOfWork.Context.Database.ExecuteSqlRaw(sql);

                        // 3. Re-enable (and re-trust) the same FKs
                        _unitOfWork.Context.Database.ExecuteSqlRaw(@"
                            DECLARE @sql nvarchar(max) = N'';
                            SELECT @sql = @sql + 
                                  N'ALTER TABLE ' 
                                + QUOTENAME(OBJECT_SCHEMA_NAME(fkc.parent_object_id))
                                + N'.' + QUOTENAME(OBJECT_NAME(fkc.parent_object_id))
                                + N' WITH CHECK CHECK CONSTRAINT ' + QUOTENAME(fk.name) + N';'
                            FROM   sys.foreign_keys fk
                            JOIN   sys.foreign_key_columns fkc
                                     ON fkc.constraint_object_id = fk.object_id
                            WHERE  fk.referenced_object_id = OBJECT_ID({0});
                            EXEC (@sql);", liveTable);
                    }
                }

                _unitOfWork.Context.DeleteAll<SimulationOutputEntity>(_ => _.RunId == simulationRunId);

                //recycle the boundary we just freed
                _unitOfWork.Context.Database.ExecuteSqlRaw(@"EXEC dbo.usp_RecycleFreedRunPartition @OldRunId = {0};", simulationRunId);

            }
            catch (Exception ex)
            {
                _log.Error($"Error during delete (SWITCH OUT) process for SimulationRunId {simulationRunId}. Rolling back transaction. Message: {ex}");
                _unitOfWork.Rollback();
            }

            _unitOfWork.Commit();
            _log.Information($"Delete completed. Committed transaction for DeleteSimulationDataByRunId (RunId: {simulationRunId}).");

            string Quote(string twoPart)
            {
                var parts = twoPart.Split('.');
                return $"[{parts[0]}].[{parts[1]}]";
            }
        }

        public void CreateSimulationOutputViaJson(Guid simulationId, SimulationOutput simulationOutput)
        {
            if (!_unitOfWork.Context.Simulation.Any(_ => _.Id == simulationId))
            {
                throw new RowNotInTableException("No simulation found for given scenario.");
            }

            var simulationEntity = _unitOfWork.Context.Simulation.AsNoTracking()
                .Single(_ => _.Id == simulationId);

            if (simulationOutput == null)
            {
                throw new InvalidOperationException($"No results found for simulation {simulationEntity.Name}. Please ensure that the simulation analysis has been run.");
            }

            var settings = new StringEnumConverter();

            try
            {
                _unitOfWork.Context.DeleteAll<SimulationOutputJsonEntity>(_ =>
                _.SimulationId == simulationId);

                var outputInitialConditionNetwork = JsonConvert.SerializeObject(simulationOutput.InitialConditionOfNetwork, settings);

                _unitOfWork.Context.Add(
                        new SimulationOutputJsonEntity
                        {
                            Id = Guid.NewGuid(),
                            SimulationId = simulationId,
                            Output = outputInitialConditionNetwork,
                            OutputType = SimulationOutputEnum.InitialConditionNetwork
                        });

                var outputInitialSummary = JsonConvert.SerializeObject(simulationOutput.InitialAssetSummaries, settings);

                _unitOfWork.Context.Add(
                        new SimulationOutputJsonEntity
                        {
                            Id = Guid.NewGuid(),
                            SimulationId = simulationId,
                            Output = outputInitialSummary,
                            OutputType = SimulationOutputEnum.InitialSummary
                        });

                foreach (var item in simulationOutput.Years)
                {
                    var targetGoalsToRemove = item.TargetConditionGoals.Where(_ => double.IsNaN(_.TargetValue) || double.IsNaN(_.ActualValue)).ToList();
                    targetGoalsToRemove.ForEach(_ => item.TargetConditionGoals.Remove(_));
                    var conditionGoalsToRemove = item.DeficientConditionGoals.Where(_ => double.IsNaN(_.DeficientLimit) || double.IsNaN(_.AllowedDeficientPercentage) || double.IsNaN(_.ActualDeficientPercentage)).ToList();
                    conditionGoalsToRemove.ForEach(_ => item.DeficientConditionGoals.Remove(_));
                    var simulationOutputYearData = JsonConvert.SerializeObject(item, settings);

                    _unitOfWork.Context.Add(
                        new SimulationOutputJsonEntity
                        {
                            Id = Guid.NewGuid(),
                            SimulationId = simulationId,
                            Output = simulationOutputYearData,
                            OutputType = SimulationOutputEnum.YearlySection
                        });
                }
                _unitOfWork.Context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static void HackSaveOutputToFile(SimulationOutput simulationOutput)
        {
            var directory = Directory.GetCurrentDirectory();
            var path = Path.Combine(directory, "SimulationOutput.json");
            var serializedOutput = JsonConvert.SerializeObject(simulationOutput);
            File.Delete(path);
            File.WriteAllText(path, serializedOutput);
        }

        /// <summary>
        /// Returns batch size from the configuration. 
        /// </summary>
        /// <param name="config">The configuration to look in</param>
        /// <param name="key">The key to use inside the configuration</param>
        /// <returns>If no batch size is found in the
        /// configuration, or if zero or less is found, returns null. Otherwise, returns the integer found.</returns>
        private static int? GetConfiguredBatchSize(IConfiguration config, string configurationKey)
        {
            var section = config.GetSection(SimulationOutputLoadKey);
            var overrideSection = section.GetSection(configurationKey);
            var overrideValue = overrideSection.Value;
            if (overrideValue != null)
            {
                if (int.TryParse(overrideValue, out int overrideInt))
                {
                    if (overrideInt > 0)
                    {
                        return overrideInt;
                    }
                }
            }
            return null;
        }

        public SimulationOutput GetSimulationOutputViaRelation(Guid simulationId, ILog loggerForUserInfo = null, ILog loggerForTechinalInfo = null, List<AttributeDTO> attributeDtos = null)
        {
            loggerForUserInfo ??= new DoNotLog();
            loggerForTechinalInfo ??= new DoNotLog();
            _unitOfWork.Context.Database.SetCommandTimeout(TimeSpan.FromSeconds(3600));
            var memos = EventMemoModelLists.GetFreshInstance("Load");
            var assetLoadBatchSize = GetConfiguredBatchSize(_unitOfWork.Config, AssetLoadBatchSizeOverrideKey) ?? AssetLoadBatchSize;
            var startMemo = memos.MarkInformation($"Starting load batchSize {assetLoadBatchSize}", loggerForTechinalInfo);
            loggerForUserInfo.Information("Loading SimulationOutput");

            if (!_unitOfWork.Context.Simulation.Any(_ => _.Id == simulationId))
            {
                throw new RowNotInTableException($"Found no simulation having id {simulationId}");
            }

            if (!_unitOfWork.Context.SimulationOutput.Any(_ => _.SimulationId == simulationId))
            {
                throw new RowNotInTableException($"No simulation analysis results were found for simulation having id {simulationId}. Please ensure that the simulation analysis has been run.");
            }

            var simulationOutputObjectCount = _unitOfWork.Context.SimulationOutput.Count(so => so.SimulationId == simulationId);
            if (simulationOutputObjectCount > 1)
            {
                throw new Exception($"Expected to find one output for the simulation. Found {simulationOutputObjectCount}."); ;
            }

            var simulationRunId = _unitOfWork.Context.SimulationOutput.Where(_ =>  _.SimulationId == simulationId).Select(_ => _.RunId).FirstOrDefault();

            var attributeNameLookup = _unitOfWork.AttributeRepo.GetAttributeNameLookupDictionary(attributeDtos);
            var entityWithoutAssetSummariesOrYearContents = _unitOfWork.Context.SimulationOutput
                .Include(so => so.Years)
                .Include(so => so.Simulation)
                .Where(_ => _.RunId == simulationRunId)
                .AsNoTracking()
                .FirstOrDefault();
            var simulationOutputId = entityWithoutAssetSummariesOrYearContents.Id;
            var cacheYears = entityWithoutAssetSummariesOrYearContents.Years.OrderBy(y => y.Year).ToList();
            entityWithoutAssetSummariesOrYearContents.Years.Clear();
            var simulationOutputDomain = SimulationOutputMapper.ToDomainWithoutAssets(entityWithoutAssetSummariesOrYearContents, attributeNameLookup);

            // AssetSummaryDetails
            #region AssetSummaryDetails
            var assetSummaryDetails = _unitOfWork.Context.AssetSummaryDetail
                .Include(a => a.MaintainableAsset)
                .OrderBy(a => a.Id)
                .Where(a => a.RunId == simulationRunId)
                .AsNoTracking()
                .ToList();
            _ = memos.Mark("assetSummaryDetails");
            var assetNameLookup = new Dictionary<Guid, string>();
            foreach (var assetSummary in assetSummaryDetails)
            {
                assetNameLookup[assetSummary.MaintainableAssetId] = assetSummary.MaintainableAsset.AssetName;
            }
            var assetSummaryDomainDictionary = AssetSummaryDetailMapper.ToDomainDictionaryNullSafe(assetSummaryDetails, attributeNameLookup);
            simulationOutputDomain.InitialAssetSummaries.AddRange(assetSummaryDomainDictionary.Values);

            // Get and map AssetSummaryDetailValuesIntId
            var assetSummaryDetailValueConfig = new BulkConfig
            {
                UpdateByProperties = new List<string> { nameof(AssetSummaryDetailValueEntityIntId.AssetSummaryDetailId), nameof(AssetSummaryDetailValueEntityIntId.AttributeId) }
            };
            var assetSummaryDetailValueEntities = new List<AssetSummaryDetailValueEntityIntId>();
            var usedAttributeIds = BuildUsedAttributeIdList(simulationOutputId);
            foreach (var assetSummaryDetail in assetSummaryDetails)
            {
                foreach (var usedAttributeId in usedAttributeIds)
                {
                    assetSummaryDetailValueEntities.Add(new AssetSummaryDetailValueEntityIntId
                    {
                        AttributeId = usedAttributeId,
                        AssetSummaryDetailId = assetSummaryDetail.Id,
                    });
                }
            }
            var configMemo = memos.MarkInformation("assetSummary config", loggerForTechinalInfo);
            _unitOfWork.Context.BulkRead(assetSummaryDetailValueEntities, assetSummaryDetailValueConfig);
            foreach (var assetSummaryDetailValueEntity in assetSummaryDetailValueEntities)
            {
                var summary = assetSummaryDomainDictionary[assetSummaryDetailValueEntity.AssetSummaryDetailId];
                AssetSummaryDetailValueMapper.AddToDictionary(assetSummaryDetailValueEntity, summary.ValuePerNumericAttribute, summary.ValuePerTextAttribute, attributeNameLookup);
            }
            // Done - Get and map AssetSummaryDetailValuesIntId
            var summariesDoneMemo = memos.MarkInformation("assetSummaries done", loggerForTechinalInfo);
            assetSummaryDetails.Clear();
            #endregion

            var allYearsWithoutAssets = _unitOfWork.Context.SimulationYearDetail
                .Where(y => y.RunId == simulationRunId)
                .Include(y => y.Budgets.Where(v => v.RunId == simulationRunId)) // This can be optional - only summary and audit reports use it
                .Include(y => y.DeficientConditionGoals.Where(v => v.RunId == simulationRunId)) // This can be optional - only general summary using it
                .Include(y => y.TargetConditionGoals.Where(v => v.RunId == simulationRunId)) // This can be optional - only general summary using it
                .AsNoTracking()
                .ToList();



            // SimulationYearDetails
            #region SimulationYearDetails
            foreach (var cacheYear in cacheYears)
            {
                var yearMemo = memos.MarkInformation($"Y{cacheYear.Year}", loggerForTechinalInfo);
                loggerForUserInfo.Information($"Loading {cacheYear.Year}");
                var yearId = cacheYear.Id;
                var year = cacheYear.Year;
                var loadedYearWithoutAssets = allYearsWithoutAssets.Where(y => y.Id == yearId).ToList();
                var loadedYearEntity = loadedYearWithoutAssets[0];
                var domainYear = SimulationYearDetailMapper.ToDomainWithoutAssets(loadedYearEntity, attributeNameLookup);
                simulationOutputDomain.Years.Add(domainYear);
                var shouldContinueLoadingAssets = true;
                var batchIndex = 0;
                var assets = new Dictionary<Guid, AssetDetail>();
                while (shouldContinueLoadingAssets)
                {
                    var assetEntities = _unitOfWork.Context.AssetDetail
                           .Where(a => a.RunId == simulationRunId)
                           .Where(a => a.SimulationYearDetailId == yearId)
                           .OrderBy(a => a.Id)
                   .AsNoTracking()
                   .Include(a => a.TreatmentConsiderations.Where(v => v.RunId == simulationRunId))
                   .ThenInclude(tc => tc.CashFlowConsiderations.Where(v => v.RunId == simulationRunId))
                   .Include(a => a.TreatmentConsiderations.Where(v => v.RunId == simulationRunId))
                   .ThenInclude(tc => tc.FundingCalculationInput)
                   .ThenInclude(fci=>fci.CurrentBudgetsToSpend.Where(v => v.RunId == simulationRunId))
                   .Include(a => a.TreatmentConsiderations.Where(v => v.RunId == simulationRunId))
                   .ThenInclude(tc => tc.FundingCalculationOutput)
                   .ThenInclude(fco=>fco.AllocationMatrix.Where(v => v.RunId == simulationRunId))
                   .Include(a => a.TreatmentOptions.Where(v => v.RunId == simulationRunId))
                   .Include(a => a.TreatmentRejections.Where(v => v.RunId == simulationRunId)) // only summary and audit reports use it
                   //.Include(a => a.TreatmentSchedulingCollisions) // no usage in reports
                   .Include(a => a.AssetDetailValuesIntId.Where(v => v.RunId == simulationRunId))
                   .AsSplitQuery()
                   .Skip(assetLoadBatchSize * batchIndex)
                   .Take(assetLoadBatchSize)
                   .ToList();
                    _ = memos.Mark("assetEntities");
                    if (assetEntities.Any())
                    {
                        AssetDetailMapper.AppendToDomainDictionaryWithValues(assets, assetEntities, year, attributeNameLookup, assetNameLookup);
                        _unitOfWork.Context.ChangeTracker.Clear();
                    }
                    _ = memos.Mark($" batch {batchIndex} done");
                    batchIndex++;
                    shouldContinueLoadingAssets = assetEntities.Count == assetLoadBatchSize;
                }
                domainYear.Assets.AddRange(assets.Values);
            }
            cacheYears.Clear();
            #endregion

            simulationOutputDomain.Years.Sort((y1, y2) => y1.Year.CompareTo(y2.Year));
            _ = memos.MarkInformation("Load done", loggerForTechinalInfo);
            loggerForUserInfo.Information($"Simulation output load completed");

            if (ShouldHackSaveTimingsToFile)
            {
                var outputFilename = "LoadTimings.txt";
                WriteTimingsToFile(memos, outputFilename);
            }
            return simulationOutputDomain;
        }

        public SimulationOutputEntity GetSimulationOutputWithoutAssetSummariesOrYearContents(Guid simulationId) => _unitOfWork.Context.SimulationOutput
                        .Include(so => so.Years)
                        .Include(so => so.Simulation)
                        .Where(_ => _.SimulationId == simulationId)
                        .AsNoTracking()
                        .FirstOrDefault();
                
        public SimulationOutput GetSimulationOutputViaJson(Guid simulationId)
        {
            if (!_unitOfWork.Context.Simulation.Any(_ => _.Id == simulationId))
            {
                throw new RowNotInTableException($"Found no simulation having id {simulationId}");
            }

            if (!_unitOfWork.Context.SimulationOutputJson.Any(_ => _.SimulationId == simulationId))
            {
                throw new RowNotInTableException($"No simulation analysis results were found for simulation having id {simulationId}. Please ensure that the simulation analysis has been run.");
            }

            var simulationOutputObjects = _unitOfWork.Context.SimulationOutputJson.Include(_ => _.Simulation).Where(_ => _.SimulationId == simulationId);

            var simulationOutput = new SimulationOutput();
            foreach (var item in simulationOutputObjects)
            {
                switch (item.OutputType)
                {
                case SimulationOutputEnum.YearlySection:
                    var yearlySections = JsonConvert.DeserializeObject<SimulationYearDetail>(item.Output, new JsonSerializerSettings
                    {
                        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                    });
                    simulationOutput.Years.Add(yearlySections);
                    break;
                case SimulationOutputEnum.InitialConditionNetwork:
                    simulationOutput.InitialConditionOfNetwork = Convert.ToDouble(item.Output);
                    break;
                case SimulationOutputEnum.InitialSummary:
                    var initialSummary = JsonConvert.DeserializeObject<List<AssetSummaryDetail>>(item.Output, new JsonSerializerSettings
                    {
                        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                    });
                    simulationOutput.InitialAssetSummaries.AddRange(initialSummary);
                    break;
                }
            }
            simulationOutput.Years.Sort((a, b) => a.Year.CompareTo(b.Year));
            simulationOutput.LastModifiedDate = simulationOutputObjects.FirstOrDefault().Simulation.LastModifiedDate;

            return simulationOutput;
        }

        public void ConvertSimulationOutpuFromJsonTorelational(Guid simulationId, CancellationToken? cancellationToken = null, IWorkQueueLog queueLogger = null)
        {
            queueLogger ??= new DoNothingWorkQueueLog();
            queueLogger.UpdateWorkQueueStatus("Getting simulation output Json");
            var output = GetSimulationOutputViaJson(simulationId);
            if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
            {
                return;
            }
            queueLogger.UpdateWorkQueueStatus("Starting conversion to relational");
            CreateSimulationOutputViaRelational(simulationId, output, queueLogger, cancellationToken: cancellationToken);
            _unitOfWork.AsTransaction(() =>
            {
                queueLogger.UpdateWorkQueueStatus("Attaching relational ouput to Json ouput");
                var outputId = _unitOfWork.Context.SimulationOutput.First(_ => _.SimulationId == simulationId).Id;
                var outputJsons = _unitOfWork.Context.SimulationOutputJson.Where(_ => _.SimulationId == simulationId).ToList();
                if (outputJsons.Count != 0)
                {
                    outputJsons.ForEach(_ => _.SimulationOutputId = outputId);
                }

                _unitOfWork.Context.UpdateAll(outputJsons);
            });
        }

        private static void WriteTimingsToFile(List<EventMemoModel> memos, string filename)
        {
            var timings = memos.ToMultilineString(true);
            System.Diagnostics.Debug.WriteLine(timings);
            var directory = Directory.GetCurrentDirectory();
            var path = Path.Combine(directory, filename);
            File.Delete(path);
            File.WriteAllText(path, timings);
        }

        private List<Guid> BuildUsedAttributeIdList(Guid simulationOutputId)
        {
            var usedAttributeIds = new List<Guid>();
            var randomAssetSummary = _unitOfWork.Context.AssetSummaryDetail
                .Include(a => a.AssetSummaryDetailValuesIntId)
                .Include(a => a.MaintainableAsset)
                .Where(a => a.SimulationOutputId == simulationOutputId)
                .AsNoTracking()
                .FirstOrDefault();
            foreach (var assetSummaryDetailValue in randomAssetSummary.AssetSummaryDetailValuesIntId)
            {
                usedAttributeIds.Add(assetSummaryDetailValue.AttributeId);
            }
            return usedAttributeIds;
        }

        public void DeleteScenarioOutputsWithingDaterange(DateTime? lowerBoundDate, DateTime upperBoundDate, CancellationToken token = default(CancellationToken))
        {
            upperBoundDate = upperBoundDate.At(hour: 23, min: 59);
            try
            {
                _unitOfWork.BeginTransaction();
                _unitOfWork.Context.DeleteAllByBatchAsync<SimulationOutputJsonEntity>(_ => (upperBoundDate == lowerBoundDate && _.CreatedDate == upperBoundDate) ||
                (lowerBoundDate == null && _.CreatedDate <= upperBoundDate) ||
                (lowerBoundDate != null && upperBoundDate > lowerBoundDate && _.CreatedDate >= lowerBoundDate && _.CreatedDate <= upperBoundDate), 100, token).Wait();
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw;
            }
            
            var outputIds = _unitOfWork.Context.SimulationOutput.Where(_ => (upperBoundDate == lowerBoundDate && _.CreatedDate == upperBoundDate) ||
                (lowerBoundDate == null && _.CreatedDate <= upperBoundDate) ||
                (lowerBoundDate != null && upperBoundDate > lowerBoundDate && _.CreatedDate >= lowerBoundDate && _.CreatedDate <= upperBoundDate)).Select(_ => _.RunId).ToList();

            foreach (var id in outputIds)
            {
                DeleteSimulationDataByRunId(id, _log);
            }
        }

        public SimulationOutputDTO GetSimulationOutput(Guid simulationId)
        {
            _unitOfWork.Context.Database.SetCommandTimeout(TimeSpan.FromSeconds(3600));            
            var assetLoadBatchSize = GetConfiguredBatchSize(_unitOfWork.Config, AssetLoadBatchSizeOverrideKey) ?? AssetLoadBatchSize;

            if (!_unitOfWork.Context.Simulation.Any(_ => _.Id == simulationId))
            {
                throw new RowNotInTableException("No simulation was found for the given scenario.");
            }

            if (!_unitOfWork.Context.SimulationOutput.Any(_ => _.SimulationId == simulationId))
            {
                return new SimulationOutputDTO
                {
                    Id = Guid.NewGuid()
                };
            }

            var simulationOutput = _unitOfWork.Context.SimulationOutput
                .Include(_ => _.InitialAssetSummaries)
                    .ThenInclude(_ => _.AssetSummaryDetailValuesIntId).FirstOrDefault(_ => _.SimulationId == simulationId);

            var simulationOutputDto = simulationOutput.ToDtoWithoutYears();

            // Years data, then ToDo per year...add to main DTO
            var yearsWithoutAssets = _unitOfWork.Context.SimulationYearDetail
                .Include(y => y.Budgets)
                .Include(y => y.DeficientConditionGoals)
                .Include(y => y.TargetConditionGoals)
                .Where(y => y.SimulationOutputId == simulationOutput.Id)
                .AsNoTracking()
                .ToList();

            foreach(var year in yearsWithoutAssets)
            {
                // Assets
                var shouldContinueLoadingAssets = true;
                var batchIndex = 0;

                while (shouldContinueLoadingAssets)
                {
                    var assetEntities = _unitOfWork.Context.AssetDetail
                           .Where(a => a.SimulationYearDetailId == year.Id)
                           .OrderBy(a => a.Id)
                   .AsNoTracking()
                   .Include(a => a.TreatmentConsiderations)
                   .ThenInclude(tc => tc.CashFlowConsiderations)
                   .Include(a => a.TreatmentConsiderations)
                   .ThenInclude(tc => tc.FundingCalculationInput)
                   .ThenInclude(fci => fci.CurrentBudgetsToSpend)
                   .Include(a => a.TreatmentConsiderations)
                   .ThenInclude(tc => tc.FundingCalculationOutput)
                   .ThenInclude(fco => fco.AllocationMatrix)
                   .Include(a => a.TreatmentOptions)
                   .Include(a => a.TreatmentRejections)
                   //.Include(a => a.TreatmentSchedulingCollisions) // no usage in reports
                   .Include(a => a.AssetDetailValuesIntId)
                   .AsSplitQuery()
                   .Skip(assetLoadBatchSize * batchIndex)
                   .Take(assetLoadBatchSize)
                   .ToList();
                    if (assetEntities.Any())
                    {                        
                        year.Assets = assetEntities;
                        _unitOfWork.Context.ChangeTracker.Clear();
                    }

                    simulationOutputDto.Years.Add(year.ToDto());

                    batchIndex++;
                    shouldContinueLoadingAssets = assetEntities.Count == assetLoadBatchSize;
                }
            }

            return simulationOutputDto;
        }

        public void CreateSimulationOutputRelational(SimulationOutputEntity simulationOutputEntity)
        {
            var simulationOutputEntityWithoutAssetsOrYearsDetails = new SimulationOutputEntity
            {
                Id = simulationOutputEntity.Id,
                InitialConditionOfNetwork = simulationOutputEntity.InitialConditionOfNetwork,
                SimulationId = simulationOutputEntity.SimulationId,
                Years = new List<SimulationYearDetailEntity>(),
                InitialAssetSummaries = new List<AssetSummaryDetailEntity>(),
            };
            _ = _unitOfWork.Context.Add(simulationOutputEntityWithoutAssetsOrYearsDetails);

            var configuredBatchSize = GetConfiguredBatchSize(_unitOfWork.Config, AssetDetailSaveOverrideBatchSizeKey);
            var batchSize = configuredBatchSize ?? AssetDetailSaveBatchSize;

            var assetSummaryDetailEntityFamily = new AssetSummaryDetailEntityFamily();
            var assetSummaries = simulationOutputEntity.InitialAssetSummaries.ToList();
            foreach (var assetSummary in assetSummaries)
            {
                var assetSummaryEntity = new AssetSummaryDetailEntity
                {
                    Id = assetSummary.Id,
                    MaintainableAssetId = assetSummary.MaintainableAssetId,
                    SimulationOutputId = assetSummary.SimulationOutputId
                };
                assetSummaryDetailEntityFamily.AssetSummaryDetails.Add(assetSummaryEntity);

                assetSummaryDetailEntityFamily.AssetSummaryDetailValues.AddRange(assetSummary.AssetSummaryDetailValuesIntId);
            }
            _unitOfWork.Context.AddAll(assetSummaryDetailEntityFamily.AssetSummaryDetails, batchSize: batchSize);
            _unitOfWork.Context.AddAll(assetSummaryDetailEntityFamily.AssetSummaryDetailValues, batchSize: batchSize);

            foreach (var year in simulationOutputEntity.Years)
            {
                var yearDetailEntity = new SimulationYearDetailEntity
                {
                    Id = year.Id,
                    Year = year.Year,
                    ConditionOfNetwork = year.ConditionOfNetwork,
                    SimulationOutputId = year.SimulationOutputId,
                    Budgets = year.Budgets,
                    DeficientConditionGoals = year.DeficientConditionGoals,
                    TargetConditionGoals = year.TargetConditionGoals
                };
                _ = _unitOfWork.Context.Add(yearDetailEntity);

                var assetFamily = new AssetDetailEntityFamily();
                var assets = year.Assets;
                foreach (var asset in assets)
                {
                    var assetDetailEntity = new AssetDetailEntity
                    {
                        Id = asset.Id,
                        AppliedTreatment = asset.AppliedTreatment,
                        MaintainableAssetId = asset.MaintainableAssetId,
                        ProjectSource = asset.ProjectSource,
                        SimulationYearDetailId = asset.SimulationYearDetailId,
                        TreatmentCause = asset.TreatmentCause,
                        TreatmentFundingIgnoresSpendingLimit = asset.TreatmentFundingIgnoresSpendingLimit,
                        TreatmentStatus = asset.TreatmentStatus
                    };
                    assetFamily.AssetDetails.Add(assetDetailEntity);

                    assetFamily.AssetDetailValues.AddRange(asset.AssetDetailValuesIntId);
                    assetFamily.TreatmentOptions.AddRange(asset.TreatmentOptions);
                    assetFamily.TreatmentRejections.AddRange(asset.TreatmentRejections);
                    assetFamily.TreatmentSchedulingCollisions.AddRange(asset.TreatmentSchedulingCollisions);

                    foreach (var treatmentConsideration in asset.TreatmentConsiderations)
                    {
                        var treatmentConsiderationDetailEntity = new TreatmentConsiderationDetailEntity
                        {
                            Id = treatmentConsideration.Id,
                            AssetDetailId = treatmentConsideration.AssetDetailId,
                            BudgetPriorityLevel = treatmentConsideration.BudgetPriorityLevel,
                            TreatmentName = treatmentConsideration.TreatmentName
                        };
                        assetFamily.TreatmentConsiderations.Add(treatmentConsiderationDetailEntity);

                        assetFamily.CashFlowConsiderations.AddRange(treatmentConsideration.CashFlowConsiderations);

                        var fundingCalculationInput = treatmentConsideration.FundingCalculationInput;
                        var fundingCalculationInputEntity = new FundingCalculationInput
                        {
                            Id = treatmentConsideration.FundingCalculationInput.Id,
                            TreatmentConsiderationDetailId = fundingCalculationInput.TreatmentConsiderationDetailId
                        };
                        assetFamily.FundingCalculationInputs.Add(fundingCalculationInputEntity);
                        assetFamily.CurrentBudgetsToSpend.AddRange(fundingCalculationInput.CurrentBudgetsToSpend);

                        var fundingCalculationOutput = treatmentConsideration.FundingCalculationOutput;
                        var fundingCalculationOutputEntity = new FundingCalculationOutput
                        {
                            Id = fundingCalculationOutput.Id,
                            TreatmentConsiderationDetailId = fundingCalculationOutput.TreatmentConsiderationDetailId
                        };
                        assetFamily.FundingCalculationOutputs.Add(fundingCalculationOutputEntity);
                        assetFamily.AllocationMatrix.AddRange(fundingCalculationOutput.AllocationMatrix);
                    }
                }

                _unitOfWork.Context.AddAll(assetFamily.AssetDetails, batchSize: batchSize);

                _unitOfWork.Context.AddAll(assetFamily.AssetDetailValues, batchSize: batchSize);

                _unitOfWork.Context.AddAll(assetFamily.TreatmentOptions, batchSize: batchSize);

                _unitOfWork.Context.AddAll(assetFamily.TreatmentRejections, batchSize: batchSize);

                _unitOfWork.Context.AddAll(assetFamily.TreatmentSchedulingCollisions, batchSize: batchSize);

                _unitOfWork.Context.AddAll(assetFamily.TreatmentConsiderations, batchSize: batchSize);

                _unitOfWork.Context.AddAll(assetFamily.FundingCalculationInputs, batchSize: batchSize);

                _unitOfWork.Context.AddAll(assetFamily.CurrentBudgetsToSpend, batchSize: batchSize);

                _unitOfWork.Context.AddAll(assetFamily.FundingCalculationOutputs, batchSize: batchSize);

                _unitOfWork.Context.AddAll(assetFamily.AllocationMatrix, batchSize: batchSize);

                _unitOfWork.Context.AddAll(assetFamily.CashFlowConsiderations, batchSize: batchSize);
            }
        }
    }
}
