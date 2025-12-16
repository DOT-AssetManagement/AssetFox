using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.DataPersistenceCore;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DTOs;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class AssetSummaryDetailMapper
    {
        public static AssetSummaryDetailEntityFamily ToEntityLists(List<AssetSummaryDetail> domainList, Guid simulationOutputId, Dictionary<string, Guid> attributeIdLookup, int simulationRunId)
        {
            var family = new AssetSummaryDetailEntityFamily();
            foreach (var domain in domainList)
            {
                AddToFamily(family, domain, simulationOutputId, attributeIdLookup, simulationRunId);
            }
            return family;
        }

        private static void AddToFamily(
            AssetSummaryDetailEntityFamily family,
            AssetSummaryDetail domain,
            Guid simulationOutputId,
            Dictionary<string, Guid> attributeIdLookup,
            int runId)
        {
            var id = SequentialGuid.NewGuid();
            var mapNumericValues = AssetSummaryDetailValueMapper.ToNumericEntityList(
                id,
                domain.ValuePerNumericAttribute,
                attributeIdLookup,
                runId);
            family.AssetSummaryDetailValues.AddRange(mapNumericValues);
            var mapTextValues = AssetSummaryDetailValueMapper.ToTextEntityList(
                id,
                domain.ValuePerTextAttribute,
                attributeIdLookup,
                runId);
            family.AssetSummaryDetailValues.AddRange(mapTextValues);
            var entity = new AssetSummaryDetailEntity
            {
                Id = id,
                SimulationOutputId = simulationOutputId,
                MaintainableAssetId = domain.AssetId,
                RunId = runId
            };
            family.AssetSummaryDetails.Add(entity);
        }

        public static AssetSummaryDetail ToDomain(AssetSummaryDetailEntity entity, Dictionary<Guid, string> attributeNameLookup)
        {
            var assetName = entity.MaintainableAsset.AssetName;
            var domain = new AssetSummaryDetail(assetName, entity.MaintainableAssetId);
            AssetSummaryDetailValueMapper.AddToDictionaries(
                entity.AssetSummaryDetailValuesIntId,
                domain.ValuePerNumericAttribute,
                domain.ValuePerTextAttribute,
                attributeNameLookup);
            return domain;
        }

        public static Dictionary<Guid, AssetSummaryDetail> ToDomainDictionaryNullSafe(ICollection<AssetSummaryDetailEntity> entityList, Dictionary<Guid, string> attributeNameLookup)
        {
            var domainDictionary = new Dictionary<Guid, AssetSummaryDetail>();
            if (entityList != null)
            {
                foreach (var entity in entityList)
                {
                    var domain = ToDomain(entity, attributeNameLookup);
                    domainDictionary[entity.Id] = domain;
                }
            }
            return domainDictionary;
        }

        public static AssetSummaryDetailDTO ToDto(this AssetSummaryDetailEntity entity)
        {
            var dto = new AssetSummaryDetailDTO
            {
                Id = entity.Id,
                MaintainableAssetId = entity.MaintainableAssetId,                
                AssetSummaryDetailValuesIntId = entity.AssetSummaryDetailValuesIntId.Select(_ => _.ToDto()).ToList()                
            };

            return dto;
        }

        public static AssetSummaryDetailEntity ToEntity(this AssetSummaryDetailDTO assetSummaryDetailDto, Guid simulationOutputId, int simulationRunId)
        {
            var assetSummaryDetailId = assetSummaryDetailDto.Id;

            return new AssetSummaryDetailEntity
            {
                Id = assetSummaryDetailId,
                SimulationOutputId = simulationOutputId,
                RunId = simulationRunId,
                MaintainableAssetId = assetSummaryDetailDto.MaintainableAssetId,
                AssetSummaryDetailValuesIntId = assetSummaryDetailDto.AssetSummaryDetailValuesIntId.Select(_ => _.ToEntity(assetSummaryDetailId)).ToList()
            };
        }
    }
}
