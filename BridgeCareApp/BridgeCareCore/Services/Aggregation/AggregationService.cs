using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.Common;
using AppliedResearchAssociates.iAM.Data;
using AppliedResearchAssociates.iAM.Data.Aggregation;
using AppliedResearchAssociates.iAM.Data.Attributes;
using AppliedResearchAssociates.iAM.Data.Helpers;
using AppliedResearchAssociates.iAM.Data.Mappers;
using AppliedResearchAssociates.iAM.Data.Networking;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Migrations;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.DTOs.Abstract;
using Microsoft.Extensions.DependencyInjection;
using NuGet.ContentModel;
using Writer = System.Threading.Channels.ChannelWriter<BridgeCareCore.Services.Aggregation.AggregationStatusMemo>;

namespace BridgeCareCore.Services.Aggregation
{
    public class AggregationService : IAggregationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILog _log;
        public AggregationService(IUnitOfWork unitOfWork, ILog log)
        {
            _unitOfWork = unitOfWork;
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>AggregationState can be just new AggregationState() object. Purpose is to allow calling class to access the state.</summary>
        public async Task<bool> AggregateNetworkData(Writer writer, Guid networkId, AggregationState state, List<AttributeDTO> attributes, CancellationToken? cancellationToken = null)
        {
            state.NetworkId = networkId;
            var isError = false;
            var isUnmatchedDatum = false;
            state.ErrorMessage = "";

            await Task.Run(() =>
            {
                var stopwatch = Stopwatch.StartNew();

                _log.Information($"Starting Aggregation.");

                try
                {

                    var maintainableAssets = new List<MaintainableAsset>();
                    var attributeData = new List<IAttributeDatum>();
                    var attributeIdsToBeUpdatedWithAssignedData = new List<Guid>();
                    if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
                    {
                        _unitOfWork.Rollback();
                        return;
                    }
                    state.Status = "Preparing";
                    _unitOfWork.NetworkRepo.UpsertNetworkRollupDetail(networkId, state.Status);  // DbUpdateException here -- "The wait operation timed out."

                    // Get/create configurable attributes
                    var configurationAttributes = AttributeDtoDomainMapper.ToDomainList(attributes, _unitOfWork.EncryptionKey);

                    var checkForDuplicateIDs = configurationAttributes.Select(_ => _.Id).ToList();

                    if (checkForDuplicateIDs.Count != checkForDuplicateIDs.Distinct().ToList().Count)
                    {
                        var networkName = _unitOfWork.NetworkRepo.GetNetworkNameOrId(networkId);
                        var broadcastError = $"Error : Duplicate attribute ids for {networkName}.";
                        WriteError(writer, broadcastError);
                        throw new InvalidOperationException();
                    }

                    var checkForDuplicateNames = configurationAttributes.Select(_ => _.Name).ToList();
                    if (checkForDuplicateNames.Count != checkForDuplicateNames.Distinct().ToList().Count)
                    {
                        var networkName = _unitOfWork.NetworkRepo.GetNetworkNameOrId(networkId);
                        var broadcastError = $"Error : Duplicate attribute names for {networkName}";
                        WriteError(writer, broadcastError);
                        throw new InvalidOperationException();
                    }

                    // get all maintainable assets in the network with their assigned data (if any) and locations
                    maintainableAssets = _unitOfWork.MaintainableAssetRepo
                        .GetAllInNetworkWithAssignedDataAndLocations(networkId)
                        .ToList();

                    // Create list of attribute ids we are allowed to update with assigned data.
                    // Could hack it in, but what is the natural way to set it up?
                    var networkAttributeIds = maintainableAssets
                        .Where(_ => _.AssignedData != null && _.AssignedData.Any())
                        .SelectMany(_ => _.AssignedData.Select(__ => __.Attribute.Id).Distinct()).ToList();

                    // create list of attribute data from configuration attributes (exclude attributes
                    // that don't have command text as there will be no way to select data for them from
                    // the data source)
                    stopwatch.Stop();
                    _log.Information($"Finished prework agg. {stopwatch.ElapsedMilliseconds}ms");
                    stopwatch.Start();

                    var attributesByDataSource = new Dictionary<BaseDataSourceDTO, List<IAttributeDatum>>();

                    try
                    {

                        var uniqueDataSources = attributes
                            .Where(attr => attr.DataSource != null)
                            .Select(attr => attr.DataSource)
                            .DistinctBy(ds => ds.Id)
                            .ToList();

                        foreach (var dataSource in uniqueDataSources)
                        {
                            //var dataSourceAttributes = attributes.Where(_ => _.DataSource == dataSource);
                            var dataSourceConfigurationAttributes = configurationAttributes.Where(_ => dataSource.Id == _.DataSourceId);

                            if (dataSource.Type == "Excel")
                            {
                                var excelSpreadsheet = _unitOfWork.ExcelWorksheetRepository.GetExcelRawDataByDataSourceId(dataSource.Id);

                                foreach (var attribute in dataSourceConfigurationAttributes)
                                {
                                    var specificData = AttributeDataBuilder
                                        .GetData(AttributeConnectionBuilder.Build(attribute, dataSource, _unitOfWork, excelSpreadsheet));
                                    attributeData.AddRange(specificData);
                                }
                            }
                            else if (dataSource.Type == "SQL")
                            {
                                foreach (var attribute in dataSourceConfigurationAttributes)
                                {
                                    var specificData = AttributeDataBuilder
                                        .GetData(AttributeConnectionBuilder.Build(attribute, dataSource, _unitOfWork));
                                    attributeData.AddRange(specificData);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        var networkName = _unitOfWork.NetworkRepo.GetNetworkNameOrId(networkId);
                        var broadcastError = $"Error: Fetching data for the attributes for {networkName}::{e.Message}";
                        WriteError(writer, broadcastError);
                        isError = true;
                        state.ErrorMessage = e.Message;
                        throw new Exception(e.StackTrace);
                    }

                    stopwatch.Stop();
                    _log.Information($"Finished attribute data. {stopwatch.ElapsedMilliseconds}ms");
                    stopwatch.Start();
                    // get the attribute ids for assigned data that can be deleted (attribute is present
                    // in the data source and meta data file)
                    attributeIdsToBeUpdatedWithAssignedData = configurationAttributes.Select(_ => _.Id)
                        .Intersect(networkAttributeIds).Distinct().ToList();

                    var aggregatedResults = new List<IAggregatedResult>();

                    var totalAssets = (double)maintainableAssets.Count;
                    var count = 0.0;

                    var directory = Directory.GetCurrentDirectory();
                    var path = Path.Combine(directory, "Logs");
                    // Set up the log
                    //var stringBuilder = new StringBuilder();
                    //stringBuilder.AppendLine("Datum Name, Location Id, Datum Id");
                    //StreamWriter streamWriter = new StreamWriter(path + "\\UnmatchedDatum.txt");
                    if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
                    {
                        _unitOfWork.Rollback();
                        return;
                    }
                    state.Status = "Aggregating";
                    _unitOfWork.NetworkRepo.UpsertNetworkRollupDetail(networkId, state.Status);

                    stopwatch.Stop();
                    _log.Information($"Starting agg step. {stopwatch.ElapsedMilliseconds}ms");
                    stopwatch.Start();

                    var attributeDataByLocationIdentifier = attributeData
                        .GroupBy(d => d.Location.LocationIdentifier)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    // loop over maintainable assets and remove assigned data that has an attribute id
                    // in attributeIdsToBeUpdatedWithAssignedData then assign the new attribute data
                    // that was created

                    foreach (var maintainableAsset in maintainableAssets)
                    {
                        if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
                        {
                            _unitOfWork.Rollback();
                            return;
                        }
                        if (count % 500 == 0)
                        {
                            state.Percentage = Math.Round(count / totalAssets * 100, 1);
                        }
                        count++;
                        maintainableAsset.AssignedData.RemoveAll(_ =>
                            attributeIdsToBeUpdatedWithAssignedData.Contains(_.Attribute.Id));
                        //List<DatumLog> unmatchedDatum = maintainableAsset.AssignAttributeData(attributeData);
                        string locationIdentifier = maintainableAsset.Location.LocationIdentifier;

                        // Direct lookup by location identifier
                        if (attributeDataByLocationIdentifier.TryGetValue(locationIdentifier, out var matchingData))
                        {
                            // Filter just to make sure they're the right type
                            var correctTypeData = matchingData.Where(d => d.Location.GetType() == maintainableAsset.Location.GetType());
                            maintainableAsset.AssignedData.AddRange(correctTypeData);
                        }
                        try
                        {
                            // aggregate numeric data
                            if (maintainableAsset.AssignedData.Any(_ => _.Attribute.DataType == "NUMBER"))
                            {
                                aggregatedResults.AddRange(maintainableAsset.AssignedData
                                    .Where(_ => _.Attribute.DataType == "NUMBER")
                                    .Select(_ => _.Attribute).Distinct()
                                    .Select(_ =>
                                        maintainableAsset.GetAggregatedValuesByYear(_,
                                            AggregationRuleFactory.CreateNumericRule(_)))
                                    .ToList());
                            }

                            //aggregate text data
                            if (maintainableAsset.AssignedData.Any(_ => _.Attribute.DataType == "STRING"))
                            {
                                aggregatedResults.AddRange(maintainableAsset.AssignedData
                                    .Where(_ => _.Attribute.DataType == "STRING")
                                    .Select(_ => _.Attribute).Distinct()
                                    .Select(_ => maintainableAsset.GetAggregatedValuesByYear(_,
                                        AggregationRuleFactory.CreateTextRule(_))).ToList());
                            }
                        }
                        catch (Exception e)
                        {
                            var networkName = _unitOfWork.NetworkRepo.GetNetworkNameOrId(networkId);
                            var broadcastError = $"Error: Creating aggregation rule(s) for the attributes for {networkName}:: {e.Message}";
                            WriteError(writer, broadcastError);
                            throw;
                        }
                    }

                    _unitOfWork.BeginTransaction();

                    if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
                    {
                        _unitOfWork.Rollback();
                        return;
                    }
                    state.Status = "Saving";
                    _unitOfWork.NetworkRepo.UpsertNetworkRollupDetail(networkId, state.Status);

                    stopwatch.Stop();
                    _log.Information($"Finished Agg Results. Start Att Datum. {stopwatch.ElapsedMilliseconds}ms");
                    stopwatch.Start();

                    //streamWriter.Close();

                    try
                    {
                        //_unitOfWork.AttributeDatumRepo.AddAssignedData(maintainableAssets, attributes);
                    }
                    catch (Exception e)
                    {
                        var networkName = _unitOfWork.NetworkRepo.GetNetworkNameOrId(networkId);
                        var broadcastError = $"Error while filling Assigned Data for {networkName} -  {e.Message}";
                        WriteError(writer, broadcastError);
                        isError = true;
                        state.ErrorMessage = e.Message;
                        throw new Exception(e.StackTrace);
                    }
                    stopwatch.Stop();
                    _log.Information($"Finished Att datum. Start save. {stopwatch.ElapsedMilliseconds}ms");
                    stopwatch.Start();
                    try
                    {
                        _unitOfWork.MaintainableAssetRepo.UpdateMaintainableAssetsSpatialWeighting(maintainableAssets);
                    }
                    catch (Exception e)
                    {
                        var networkName = _unitOfWork.NetworkRepo.GetNetworkNameOrId(networkId);
                        var broadcastError = $"Error while Updating MaintainableAssets SpatialWeighting for {networkName} -  {e.Message}";
                        WriteError(writer, broadcastError);
                        isError = true;
                        state.ErrorMessage = e.Message;
                        throw new Exception(e.StackTrace);
                    }
                    try
                    {
                        _unitOfWork.AggregatedResultRepo.AddAggregatedResults(aggregatedResults);
                    }
                    catch (Exception e)
                    {
                        var networkName = _unitOfWork.NetworkRepo.GetNetworkNameOrId(networkId);
                        var broadcastError = $"Error while adding Aggregated results for {networkName} -  {e.Message}";
                        WriteError(writer, broadcastError);
                        isError = true;
                        state.ErrorMessage = e.Message;
                        throw new Exception(e.StackTrace);
                    }
                    if (!isError)
                    {
                        state.Status = "Aggregated all network data";
                        _unitOfWork.NetworkRepo.UpsertNetworkRollupDetail(networkId, state.Status);
                        _unitOfWork.Commit();

                        WriteState(writer, state);
                    }
                    else
                    {
                        state.Status = $"Error in data aggregation {state.ErrorMessage}";
                        _unitOfWork.Rollback();
                        state.Percentage = 0;
                        WriteState(writer, state);
                    }
                    stopwatch.Stop();
                    _log.Information($"Finished Agg. {stopwatch.ElapsedMilliseconds}ms");

                }
                catch
                {
                    _unitOfWork.Rollback();
                    throw;
                }

            });
            if (isUnmatchedDatum)
            {
                WriteError(writer, "Unmatched Datum locations found::See unmatchedDatum.txt log file for more details.");
            }
            return !isError;
        }

        private static void WriteError(Writer writer, string error)
        {
            var status = new AggregationStatusMemo
            {
                ErrorMessage = error,
            };
            writer.TryWrite(status);
        }

        private static void WriteState(Writer writer, AggregationState state)
        {
            var dto = new NetworkRollupDetailDTO
            {
                NetworkId = state.NetworkId,
                Status = state.Status,
            };
            var memo = new AggregationStatusMemo
            {
                ErrorMessage = null,
                Percentage = state.Percentage,
                rollupDetailDto = dto,
            };
            writer.TryWrite(memo);
        }
    }
}
