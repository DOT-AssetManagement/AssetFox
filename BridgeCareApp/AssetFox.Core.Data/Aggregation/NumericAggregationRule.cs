using System.Collections.Generic;
using AssetFox.Core.Data.Attributes;

namespace AssetFox.Core.Data.Aggregation
{
    public abstract class NumericAggregationRule : AggregationRule<double>
    {
        public abstract override IEnumerable<(Attribute attribute, (int year, double value))> Apply(IEnumerable<IAttributeDatum> attributeData, Attribute attribute);
    }
}
