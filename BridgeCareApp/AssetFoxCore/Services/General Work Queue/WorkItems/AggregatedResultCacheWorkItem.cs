using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using AssetFox.Core.Common.PerformanceMeasurement;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs.Enums;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Hubs;
using AssetFox.Core.WorkQueue;
using AssetFoxCore.Models;
using HotChocolate.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Models;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.Common;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL;

namespace AssetFoxCore.Services
{
    public class AggregatedResultCacheWorkItem : IWorkSpecification<WorkQueueMetadata>
    {
        public string UserId => "system";

        public readonly Guid Id;

        public string WorkId => WorkQueueWorkIdFactory.CreateId(Id, WorkType.Aggregation);

        public string WorkDescription => "Rebuild aggregated result cache";

        public string WorkName => WorkDescription;

        public AggregatedResultCacheWorkItem()
        {
            Id = Guid.NewGuid();
        }

        public WorkQueueMetadata Metadata => new()
        {
            WorkType = WorkType.SimulationAnalysis,
            DomainType = DomainType.Network,
            DomainId = Guid.Empty,
        };

        public void DoWork(IServiceProvider serviceProvider, Action<string> updateStatusOnHandle, CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            DoWorkInner(scope);
        }

        private static void DoWorkInner(IServiceScope scope)
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>() as UnitOfDataPersistenceWork;
            var logger = scope.ServiceProvider.GetRequiredService<ILog>();
            logger.Information("Starting aggregated result value cache work item");
            var attributeRepository = unitOfWork.AttributeRepo;
            var aggregatedResultRepository = unitOfWork.AggregatedResultRepo;
            var primaryNetwork = unitOfWork.AdminSettingsRepo.GetPrimaryNetworkId();
            if (primaryNetwork == null)
            {
                logger.Information("No primary network defined. Aborting cache job.");
            }
            else 
            {
                var allAttributes = attributeRepository.GetAttributes();
                var allNames = allAttributes.Select(a => a.Name).ToList();
                var cache = scope.ServiceProvider.GetRequiredService<IAggregatedSelectValuesResultDtoCache>();
                var tooBig = cache.AttributesTooBigToCache;
                var attributesToCache = allNames.Except(tooBig).ToList();
                var batches = new List<List<string>>();
                var batchSize = 10;
                while (attributesToCache.Any())
                {
                    var batch = attributesToCache.Take(batchSize)
                        .ToList();
                    batches.Add(batch);
                    attributesToCache = attributesToCache.Skip(batchSize).ToList();
                }
                foreach (var batch in batches)
                {
                    var allDtos = aggregatedResultRepository.GetAggregatedResultsForAttributeNames(batch);
                    foreach (var dto in allDtos)
                    {
                        cache.SaveToCache(dto);
                    }
                }
            }
        }

        public void OnCompletion(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            var message = $"Attribute value cache rebuilt {DateTime.Now}";
            _hubService.SendRealTimeMessage("system", HubConstant.BroadcastTaskCompleted, message);
            var logger = serviceProvider.GetRequiredService<ILog>();
            logger.Information(message);
        }

        public void OnFault(IServiceProvider serviceProvider, string errorMessage)
        {
            string cacheRebuildError = "Error building attribute value cache";
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();

            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastError, $"{cacheRebuildError}::NetworkAggregateAccess - {errorMessage}");
            var logger = serviceProvider.GetRequiredService<ILog>();
            logger.Error(errorMessage);

        }
        public void OnUpdate(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();
            _hubService.SendRealTimeMessage(UserId, HubConstant.BroadcastWorkQueueUpdate, WorkId);

        }
    }
}
