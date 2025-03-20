using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.Analysis;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class AssetDetailValueMapper
    {
        public static AssetDetailValueEntityIntId ToNumericEntity(
            Guid assetDetailId,
            KeyValuePair<string, double> assetSummaryDetailValue,
            Dictionary<string, Guid> attributeIdLookupDictionary)
        {
            var attributeId = attributeIdLookupDictionary[assetSummaryDetailValue.Key];
            var entity = new AssetDetailValueEntityIntId
            {
                AssetDetailId = assetDetailId,
                Discriminator = AssetDetailValueDiscriminators.Number,
                AttributeId = attributeId,
                NumericValue = assetSummaryDetailValue.Value,
            };
            return entity;
        }

        public static AssetDetailValueEntityIntId ToTextEntity(
            Guid assetDetailId,
            KeyValuePair<string, string> assetSummaryDetailValue,
            Dictionary<string, Guid> attributeIdLookupDictionary)
        {
            var attributeId = attributeIdLookupDictionary[assetSummaryDetailValue.Key];
            var entity = new AssetDetailValueEntityIntId
            {
                AssetDetailId = assetDetailId,
                Discriminator = AssetDetailValueDiscriminators.Text,
                AttributeId = attributeId,
                TextValue = assetSummaryDetailValue.Value,
            };
            return entity;
        }

        public static List<AssetDetailValueEntityIntId> ToNumericEntityList(
            Guid assetDetailId,
            Dictionary<string, double> assetSummaryDetailValues,
            Dictionary<string, Guid> attributeIdLookup)
        {
            var entities = new List<AssetDetailValueEntityIntId>();

            foreach (var keyValuePair in assetSummaryDetailValues)
            {
                if (attributeIdLookup.ContainsKey(keyValuePair.Key))
                {
                    var entity = ToNumericEntity(assetDetailId, keyValuePair, attributeIdLookup);
                    entities.Add(entity);
                }
            }
            return entities;
        }

        public static List<AssetDetailValueEntityIntId> ToTextEntityList(
            Guid assetDetailId,
            Dictionary<string, string> assetSummaryDetailValues,
            Dictionary<string, Guid> attributeIdLookup)
        {
            var entities = new List<AssetDetailValueEntityIntId>();
            foreach (var keyValuePair in assetSummaryDetailValues)
            {
                var entity = ToTextEntity(assetDetailId, keyValuePair, attributeIdLookup);
                entities.Add(entity);
            }
            return entities;
        }

        public static void AddToDictionaries(
            ICollection<AssetDetailValueEntityIntId> entityCollection,
            Dictionary<string, string> valuePerTextAttribute,
            Dictionary<string, double> valuePerNumericAttribute,
            Dictionary<Guid, string> attributeNameLookup)
        {
            foreach (var entity in entityCollection)
            {
                AddToDictionary(entity, valuePerTextAttribute, valuePerNumericAttribute, attributeNameLookup);
            }
            // TODO check if needed and what alternative?
            FillArea(valuePerNumericAttribute);
        }

        public static void FillArea(Dictionary<string, double> valuePerNumericAttribute)
        {
            var areaKey = Network.DefaultSpatialWeightingIdentifier;
            var deckAreaKey = AttributeNameConstants.DeckArea;
            if (valuePerNumericAttribute.ContainsKey(deckAreaKey))
            {
                valuePerNumericAttribute[areaKey] = valuePerNumericAttribute[deckAreaKey];
            }
        }

        public static void AddToDictionary(
            AssetDetailValueEntityIntId entity,
            Dictionary<string, string> valuePerTextAttribute,
            Dictionary<string, double> valuePerNumericAttribute,
            Dictionary<Guid, string> attributeNameLookup)
        {
            var attributeName = attributeNameLookup[entity.AttributeId];
            // WjJake -- how should we handle unexpected cases, i.e. invalid discriminator, or discriminator is "number" but the numeric value is null?
            switch (entity.Discriminator)
            {
            case AssetDetailValueDiscriminators.Number:
                if (entity.NumericValue.HasValue)
                {
                    valuePerNumericAttribute[attributeName] = entity.NumericValue.Value;

                }
                break;
            case AssetDetailValueDiscriminators.Text:
                valuePerTextAttribute[attributeName] = entity.TextValue;
                break;
            }
        }
    }
}
