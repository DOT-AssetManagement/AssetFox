using System;

namespace AssetFox.Core.DTOs.Abstract
{
    public class BaseAttributeDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string AggregationRuleType { get; set; } // Note: removed from UI

        public string Command { get; set; } // Note: removed from UI

        public string DefaultValue { get; set; }

        public double? Minimum { get; set; }

        public double? Maximum { get; set; }

        public bool IsCalculated { get; set; }

        public bool IsAscending { get; set; }
    }
}
