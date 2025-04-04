using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
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
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

        public SimulationOutputRepository(UnitOfDataPersistenceWork unitOfWork) => _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

        public void CreateSimulationOutputViaRelational(Guid simulationId, SimulationOutput simulationOutput,
            IWorkQueueLog loggerForUserInfo = null, ILog loggerForTechnicalInfo = null, CancellationToken? cancellationToken = null)
        {
            loggerForTechnicalInfo ??= new DoNotLog();
            loggerForUserInfo ??= new DoNothingWorkQueueLog();
            loggerForUserInfo.UpdateWorkQueueStatus("Preparing to save to database");

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
            if (!_unitOfWork.Context.Simulation.Any(_ => _.Id == simulationId))
            {
                throw new RowNotInTableException("No simulation found for given scenario.");
            }

            _unitOfWork.BeginTransaction();

            // Once we are inside the transaction, there is not much value in adding logs to the
            // user logger. The problem is that these are often communicated to the user via the database.
            // But the database won't update until the transaction is completed. Therefore,
            // adding user logs about the transaction's progress will likely just cause confusion
            // about why these logs are sometimes not appearing in the UI.
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

            try
            {
                if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
                {
                    _unitOfWork.Rollback();
                    return;
                }

                // delete existing simulation outputs
                var toDelete = _unitOfWork.Context.SimulationOutput.Where(_ => _.SimulationId == simulationId).Select(_ => _.Id).ToList();
                DeleteSimulationOutputs(toDelete);
                _ = _unitOfWork.Context.SaveChanges();

                var simulationOutputEntity = SimulationOutputMapper.ToEntityWithoutAssetsOrYearDetails(simulationOutput, simulationId, attributeIdLookup);
                _ = _unitOfWork.Context.Add(simulationOutputEntity);
                _ = _unitOfWork.Context.SaveChanges();

                var configuredBatchSize = GetConfiguredBatchSize(_unitOfWork.Config, AssetDetailSaveOverrideBatchSizeKey);
                var batchSize = configuredBatchSize ?? AssetDetailSaveBatchSize;

                var assetSummaries = simulationOutput.InitialAssetSummaries;
                _ = saveMemos.Mark("assetSummaries");

                var family = AssetSummaryDetailMapper.ToEntityLists(assetSummaries, simulationOutputEntity.Id, attributeIdLookup);
                _unitOfWork.Context.AddAll(family.AssetSummaryDetails, batchSize: batchSize);
                _ = simulationMemos.Mark("assetSummaryDetails");

                _unitOfWork.Context.AddAll(family.AssetSummaryDetailValues, batchSize: batchSize);
                _= saveMemos.Mark("assetSummaryDetailValues");
                _unitOfWork.Commit();

                foreach (var year in simulationOutput.Years)
                {
                    loggerForUserInfo.UpdateWorkQueueStatus($"Saving year {year.Year}");
                    _unitOfWork.BeginTransaction();
                    if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
                    {
                        _unitOfWork.Rollback();
                        return;
                    }
                    var yearMemo = saveMemos.MarkInformation($"Y{year.Year}", loggerForTechnicalInfo);
                    var yearDetail = SimulationYearDetailMapper.ToEntityWithoutAssets(year, simulationOutputEntity.Id, attributeIdLookup);
                    _ = _unitOfWork.Context.Add(yearDetail);

                    var assets = year.Assets;
                    var assetFamily = AssetDetailMapper.ToEntityFamily(assets, yearDetail.Id, attributeIdLookup);

                    _unitOfWork.Context.AddAll(assetFamily.AssetDetails, batchSize: batchSize);
                    _ = saveMemos.Mark($" {assetFamily.AssetDetails.Count} assetDetails");

                    _unitOfWork.Context.AddAll(assetFamily.AssetDetailValues, batchSize: batchSize);
                    _ =saveMemos.Mark($" {assetFamily.AssetDetailValues.Count} assetDetailValues batchSize: {batchSize}");

                    _unitOfWork.Context.AddAll(assetFamily.TreatmentOptions, batchSize: batchSize);
                    _ = saveMemos.Mark($" {assetFamily.TreatmentOptions.Count} treatmentOptions");

                    _unitOfWork.Context.AddAll(assetFamily.TreatmentRejections, batchSize: batchSize);
                    _ = saveMemos.Mark($" {assetFamily.TreatmentRejections.Count} treatmentRejections");

                    _unitOfWork.Context.AddAll(assetFamily.TreatmentSchedulingCollisions, batchSize: batchSize);
                    _ = saveMemos.Mark($" {assetFamily.TreatmentSchedulingCollisions.Count} treatmentSchedulingCollisions");

                    _unitOfWork.Context.AddAll(assetFamily.TreatmentConsiderations, batchSize: batchSize);
                    _ = saveMemos.Mark($" {assetFamily.TreatmentConsiderations.Count} treatmentConsiderations");
                                        
                    _unitOfWork.Context.AddAll(assetFamily.FundingCalculationInputs, batchSize: batchSize);
                    _ = saveMemos.Mark($" {assetFamily.FundingCalculationInputs.Count} fundingCalculationInputs");

                    _unitOfWork.Context.AddAll(assetFamily.CurrentBudgetsToSpend, batchSize: batchSize);
                    _ = saveMemos.Mark($" {assetFamily.CurrentBudgetsToSpend.Count} currentBudgetsToSpend");

                    _unitOfWork.Context.AddAll(assetFamily.FundingCalculationOutputs, batchSize: batchSize);
                    _ = saveMemos.Mark($" {assetFamily.FundingCalculationOutputs.Count} fundingCalculationOutputs");

                    _unitOfWork.Context.AddAll(assetFamily.AllocationMatrix, batchSize: batchSize);
                    _ = saveMemos.Mark($" {assetFamily.AllocationMatrix.Count} allocationMatrix");

                    _unitOfWork.Context.AddAll(assetFamily.CashFlowConsiderations, batchSize: batchSize);
                    _ = saveMemos.Mark($" {assetFamily.CashFlowConsiderations.Count} cashFlowConsiderations");

                    _unitOfWork.Commit();
                    _ = saveMemos.Mark(" Committed");

                    _unitOfWork.Context.ChangeTracker.Clear();
                    _ = saveMemos.Mark(" Cleared ChangeTracker");
                }

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
                _unitOfWork.Rollback();
                throw;
            }
        }

        public void DeleteSimulationOutputs(List<Guid> simulationOutputIds)
        {
            if (!simulationOutputIds.Any())
            {
                return;
            }

            _unitOfWork.Context.Database.SetCommandTimeout(TimeSpan.FromSeconds(3600));

            var simulationOutputIdsStr = string.Join(",", simulationOutputIds.Select(_ => _.ToString()));
            // RegEx Explained: \s means "match any whitespace token", and + means "match one or more of the proceeding token
            simulationOutputIdsStr = System.Text.RegularExpressions.Regex.Replace(simulationOutputIdsStr, @"\s+", string.Empty);

            // Create parameters for the stored procedure
            var retMessageParam = new SqlParameter("@RetMessage", SqlDbType.VarChar, 250);
            retMessageParam.Direction = ParameterDirection.Output;
            var simGuidListParam = new SqlParameter("@SimGuidList", simulationOutputIdsStr);

            // Execute the stored procedure
            var result = _unitOfWork.Context.Database.ExecuteSqlRaw("EXEC usp_delete_simulationoutput @SimGuidList, @RetMessage OUTPUT", simGuidListParam, retMessageParam);

            // Capture the success output value
            var retMessage = retMessageParam.Value as string;
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

            var attributeNameLookup = _unitOfWork.AttributeRepo.GetAttributeNameLookupDictionary(attributeDtos);
            var entityWithoutAssetSummariesOrYearContents = _unitOfWork.Context.SimulationOutput
                .Include(so => so.Years)
                .Include(so => so.Simulation)
                .Where(_ => _.SimulationId == simulationId)
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
                .Where(a => a.SimulationOutputId == simulationOutputId)
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

            // SimulationYearDetails
            #region SimulationYearDetails
            foreach (var cacheYear in cacheYears)
            {
                var yearMemo = memos.MarkInformation($"Y{cacheYear.Year}", loggerForTechinalInfo);
                loggerForUserInfo.Information($"Loading {cacheYear.Year}");
                var yearId = cacheYear.Id;
                var year = cacheYear.Year;
                var loadedYearWithoutAssets = _unitOfWork.Context.SimulationYearDetail
                .Include(y => y.Budgets) // This can be optional - only summary and audit reports use it
                .Include(y => y.DeficientConditionGoals) // This can be optional - only general summary using it
                .Include(y => y.TargetConditionGoals) // This can be optional - only general summary using it
                .Where(y => y.Id == yearId)
                .AsNoTracking()
                .ToList();
                var loadedYearEntity = loadedYearWithoutAssets[0];
                var domainYear = SimulationYearDetailMapper.ToDomainWithoutAssets(loadedYearEntity, attributeNameLookup);
                simulationOutputDomain.Years.Add(domainYear);
                var shouldContinueLoadingAssets = true;
                var batchIndex = 0;
                var assets = new Dictionary<Guid, AssetDetail>();
                while (shouldContinueLoadingAssets)
                {
                    var assetEntities = _unitOfWork.Context.AssetDetail
                           .Where(a => a.SimulationYearDetailId == yearId)
                           .OrderBy(a => a.Id)
                   .AsNoTracking()
                   .Include(a => a.TreatmentConsiderations)
                   .ThenInclude(tc => tc.CashFlowConsiderations)
                   .Include(a => a.TreatmentConsiderations)
                   .ThenInclude(tc => tc.FundingCalculationInput)
                   .ThenInclude(fci=>fci.CurrentBudgetsToSpend)
                   .Include(a => a.TreatmentConsiderations)
                   .ThenInclude(tc => tc.FundingCalculationOutput)
                   .ThenInclude(fco=>fco.AllocationMatrix)
                   .Include(a => a.TreatmentOptions)
                   .Include(a => a.TreatmentRejections) // only summary and audit reports use it
                   //.Include(a => a.TreatmentSchedulingCollisions) // no usage in reports
                   .Include(a => a.AssetDetailValuesIntId)
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
            
            var outputids = _unitOfWork.Context.SimulationOutput.Where(_ => (upperBoundDate == lowerBoundDate && _.CreatedDate == upperBoundDate) ||
                (lowerBoundDate == null && _.CreatedDate <= upperBoundDate) ||
                (lowerBoundDate != null && upperBoundDate > lowerBoundDate && _.CreatedDate >= lowerBoundDate && _.CreatedDate <= upperBoundDate)).Select(_ => _.Id.ToString()).ToList();
            var idChunks =  outputids.Chunk(100).ToList();
            //this needs to be done because SimOutputGuidList is nvarchar and thus it can only store up to 4000 characters
            idChunks.ForEach(chunk =>
            {
                var param = new SqlParameter[] {
                        new SqlParameter() {
                            ParameterName = "@SimOutputGuidList",
                            SqlDbType =  System.Data.SqlDbType.NVarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = String.Join(",", chunk.ToArray())
                        },

                        new SqlParameter() {
                            ParameterName = "@RetMessage",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Size = 250,
                            Direction = System.Data.ParameterDirection.Output,
                        }};
                _unitOfWork.Context.Database.ExecuteSqlRawAsync("[dbo].[usp_delete_simulationoutput] @SimOutputGuidList, @RetMessage", param, token).Wait();
            });
        }
    }
}
