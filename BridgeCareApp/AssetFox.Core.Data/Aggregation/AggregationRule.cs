using System.Collections.Generic;
using AssetFox.Core.Data.Attributes;
using Attribute = AssetFox.Core.Data.Attributes.Attribute;

namespace AssetFox.Core.Data.Aggregation
{
    public abstract class AggregationRule<T>
    {
        public abstract IEnumerable<(Attribute attribute, (int year, T value))> Apply(IEnumerable<IAttributeDatum> attributeData, Attribute attribute);
    }
}
