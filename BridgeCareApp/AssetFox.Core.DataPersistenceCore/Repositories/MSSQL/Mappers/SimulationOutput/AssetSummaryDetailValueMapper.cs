using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.Analysis;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.DTOs;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class AssetSummaryDetailValueMapper
    {
        public static AssetSummaryDetailValueEntityIntId ToNumericEntity(
            Guid assetSummaryDetailId,
            KeyValuePair<string, double> assetSummaryDetailValue,
            Dictionary<string, Guid> attributeIdLookupDictionary,
            int runId)
        {
            var attributeId = attributeIdLookupDictionary[assetSummaryDetailValue.Key];
            var entity = new AssetSummaryDetailValueEntityIntId
            {
                AssetSummaryDetailId = assetSummaryDetailId,
                Discriminator = AssetDetailValueDiscriminators.Number,
                AttributeId = attributeId,
                NumericValue = assetSummaryDetailValue.Value,
                RunId = runId
            };
            return entity;
        }

        public static AssetSummaryDetailValueEntityIntId ToTextEntity(
            Guid assetSummaryDetailId,
            KeyValuePair<string, string> keyValuePair,
            Dictionary<string, Guid> attributeIdLookupDictionary,
            int runId)
        {
            var attributeId = attributeIdLookupDictionary[keyValuePair.Key];
            var entity = new AssetSummaryDetailValueEntityIntId
            {
                AssetSummaryDetailId = assetSummaryDetailId,
                Discriminator = AssetDetailValueDiscriminators.Text,
                AttributeId = attributeId,
                TextValue = keyValuePair.Value,
                RunId = runId
            };
            return entity;
        }

        internal static void AddToDictionaries(
            ICollection<AssetSummaryDetailValueEntityIntId> assetSummaryDetailValues,
            IDictionary<string, double> valuePerNumericAttribute,
            IDictionary<string, string> valuePerTextAttribute,
            Dictionary<Guid, string> attributeNameLookup)
        {
            foreach (var summary in assetSummaryDetailValues)
            {
                AddToDictionary(summary, valuePerNumericAttribute, valuePerTextAttribute, attributeNameLookup);
            }
        }

        public static void FillAreaAttributeValue(Dictionary<string, double> valuePerNumericAttribute)
        {
            var areaKey = Network.DefaultSpatialWeightingIdentifier;
            var deckAreaKey = AttributeNameConstants.DeckArea;
            if (valuePerNumericAttribute.ContainsKey(deckAreaKey))
            {
                valuePerNumericAttribute[areaKey] = valuePerNumericAttribute[deckAreaKey];
            }
        }

        public static void AddToDictionary(
            AssetSummaryDetailValueEntityIntId summary,
            IDictionary<string, double> valuePerNumericAttribute,
            IDictionary<string, string> valuePerTextAttribute,
            Dictionary<Guid, string> attributeNameLookup
            )
        {
            var attributeName = attributeNameLookup[summary.AttributeId];
            switch (summary.Discriminator)
            {
            case AssetDetailValueDiscriminators.Number:
                if (summary.NumericValue.HasValue)
                {
                    valuePerNumericAttribute[attributeName] = summary.NumericValue.Value;
                }
                break;
            case AssetDetailValueDiscriminators.Text:
                valuePerTextAttribute[attributeName] = summary.TextValue;
                break;
            }
        }

        public static List<AssetSummaryDetailValueEntityIntId> ToNumericEntityList(
            Guid assetSummaryDetailId,
            IDictionary<string, double> assetSummaryDetailValues,
            Dictionary<string, Guid> attributeIdLookup, int runId)
        {
            var entities = new List<AssetSummaryDetailValueEntityIntId>();

            foreach (var keyValuePair in assetSummaryDetailValues)
            {
                if (attributeIdLookup.ContainsKey(keyValuePair.Key))
                {
                    var entity = ToNumericEntity(assetSummaryDetailId, keyValuePair, attributeIdLookup, runId);
                    entities.Add(entity);
                }                
            }
            return entities;

        }

        public static List<AssetSummaryDetailValueEntityIntId> ToTextEntityList(
            Guid assetSummaryDetailId,
            IDictionary<string, string> assetSummaryDetailValues,
            Dictionary<string, Guid> attributeIdLookup, int runId)
        {
            var entities = new List<AssetSummaryDetailValueEntityIntId>();
            foreach (var keyValuePair in assetSummaryDetailValues)
            {
                if (attributeIdLookup.ContainsKey(keyValuePair.Key))
                {
                    var entity = ToTextEntity(assetSummaryDetailId, keyValuePair, attributeIdLookup, runId);
                    entities.Add(entity);
                }
            }
            return entities;
        }

        public static AssetSummaryDetailValueEntityIntIdDTO ToDto(this AssetSummaryDetailValueEntityIntId entity)
        {
            var dto = new AssetSummaryDetailValueEntityIntIdDTO
            {
                Id = entity.Id,
                AttributeId = entity.AttributeId,
                Discriminator = entity.Discriminator,
                NumericValue = entity.NumericValue,
                TextValue = entity.TextValue
            };

            return dto;
        }

        public static AssetSummaryDetailValueEntityIntId ToEntity(this AssetSummaryDetailValueEntityIntIdDTO assetSummaryDetailValueEntityIntIdDto, Guid assetSummaryDetailId)
        {
            return new AssetSummaryDetailValueEntityIntId
            {
                AssetSummaryDetailId = assetSummaryDetailId,
                AttributeId = assetSummaryDetailValueEntityIntIdDto.AttributeId,
                Discriminator = assetSummaryDetailValueEntityIntIdDto.Discriminator,
                NumericValue = assetSummaryDetailValueEntityIntIdDto.NumericValue,
                TextValue = assetSummaryDetailValueEntityIntIdDto.TextValue
            };
        }
    }
}
