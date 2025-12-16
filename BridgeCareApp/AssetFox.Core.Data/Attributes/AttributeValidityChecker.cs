using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetFox.Core.Data.Attributes
{
    public static class AttributeValidityChecker
    {
        public static bool IsValid(Attribute attribute)
        {
            bool returnValue = false;
            if (attribute is TextAttribute textAttribute)
            {
                returnValue = TextAttributeAggregationRules.ValidRuleNames().Any(
                    r => r.Equals(attribute.AggregationRuleType, StringComparison.OrdinalIgnoreCase));
            }
            if (attribute is NumericAttribute numericAttribute)
            {
                returnValue = NumericAttributeAggregationRules.ValidRules().Any(
                    r => r.Equals(attribute.AggregationRuleType, StringComparison.OrdinalIgnoreCase));
            }
            return returnValue;
        }
    }
}
