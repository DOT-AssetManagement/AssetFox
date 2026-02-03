using System;
using Attribute = AssetFox.Core.Data.Attributes.Attribute;

namespace AssetFox.Core.Data.Aggregation
{
    public static class AggregationRuleFactory
    {
        public static NumericAggregationRule CreateNumericRule(Attribute attribute)
        {
            //Disabling Aggregation Rules for now. Default to Predominant.
            /*
            return attribute.AggregationRuleType.ToUpper() switch
            {
                "AVERAGE" => new AverageAggregationRule(),
                "LAST" => new LastNumericAggregationRule(),
                "PREDOMINANT" => new PredominantNumericAggregationRule(),
                "ADD" => new AddAggregationRule(),
                _ => throw new InvalidOperationException(),
            };*/

            return new PredominantNumericAggregationRule();
        }

        public static TextAggregationRule CreateTextRule(Attribute attribute)
        {
            //Disabling aggregation rules for now.Default to Predominant.
            /* return attribute.AggregationRuleType.ToUpper() switch
            {
                "PREDOMINANT" => new PredominantTextAggregationRule(),
                "LAST" => new LastTextAggregationRule(),
                _ => throw new InvalidOperationException(),
            };*/

            return new PredominantTextAggregationRule();
        }
    }
}
