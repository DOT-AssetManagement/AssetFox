using System.Collections.Generic;
using AssetFox.Core.Data.Attributes;
using Attribute = AssetFox.Core.Data.Attributes.Attribute;

namespace AssetFox.Core.Data.Aggregation
{
    public abstract class TextAggregationRule : AggregationRule<string>
    {
        public abstract override IEnumerable<(Attribute attribute, (int year, string value))> Apply(IEnumerable<IAttributeDatum> attributeData, Attribute attribute);
    }
}
