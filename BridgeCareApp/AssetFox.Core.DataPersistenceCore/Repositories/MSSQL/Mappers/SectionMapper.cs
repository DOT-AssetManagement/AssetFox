using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.Analysis;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public sealed class SectionMapper
    {
        public SectionMapper(Network network)
        {
            Network = network;
            HistoryMapper = new(network);
        }

        public void CreateMaintainableAsset(MaintainableAssetEntity entity, IReadOnlyDictionary<Guid, string> attributeNameLookup)
        {
            var asset = Network.AddAsset();
            asset.Id = entity.Id;
            asset.AssetName = entity.AssetName;
            asset.SpatialWeighting.Expression = entity.SpatialWeighting;

            if (entity.AggregatedResults.Any(_ => _.Discriminator == DataPersistenceConstants.AggregatedResultNumericDiscriminator))
            {
                var numericResults = entity.AggregatedResults
                    .Where(_ => _.Discriminator == DataPersistenceConstants.AggregatedResultNumericDiscriminator)
                    .ToList();

                HistoryMapper.SetNumericAttributeValueHistories(numericResults, asset, attributeNameLookup);
            }

            if (entity.AggregatedResults.Any(_ => _.Discriminator == DataPersistenceConstants.AggregatedResultTextDiscriminator))
            {
                var textResults = entity.AggregatedResults
                    .Where(_ => _.Discriminator == DataPersistenceConstants.AggregatedResultTextDiscriminator)
                    .ToList();

                HistoryMapper.SetTextAttributeValueHistories(textResults, asset, attributeNameLookup);
            }
        }

        private readonly AttributeValueHistoryMapper HistoryMapper;
        private readonly Network Network;
    }
}
