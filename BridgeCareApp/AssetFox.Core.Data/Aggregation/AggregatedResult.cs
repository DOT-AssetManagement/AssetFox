using System;
using System.Collections.Generic;
using AssetFox.Core.Data.Networking;
using Attribute = AssetFox.Core.Data.Attributes.Attribute;

namespace AssetFox.Core.Data.Aggregation
{
    public class AggregatedResult<T> : IAggregatedResult
    {
        public AggregatedResult(Guid id, MaintainableAsset maintainableAsset, IEnumerable<(Attribute attribute, (int year, T value))> aggregatedData)
        {
            Id = id;
            MaintainableAsset = maintainableAsset;
            AggregatedData = aggregatedData;
        }

        public Guid Id { get; }

        public MaintainableAsset MaintainableAsset { get; }

        public IEnumerable<(Attribute attribute, (int year, T value) yearValuePair)> AggregatedData { get; }
    }
}
