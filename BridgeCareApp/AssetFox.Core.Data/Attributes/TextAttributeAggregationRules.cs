using System;
using System.Collections.Generic;
using System.Text;

namespace AssetFox.Core.Data.Attributes
{
    public class TextAttributeAggregationRules
    {
        public const string Predominant = "PREDOMINANT";
        public static IEnumerable<string> ValidRuleNames()
        {
            yield return Predominant;
        }
    }
}
