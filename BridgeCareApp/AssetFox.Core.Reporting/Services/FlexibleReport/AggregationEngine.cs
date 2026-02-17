using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.Reporting.Models.FlexibleReport;

namespace AssetFox.Core.Reporting.Services.FlexibleReport
{
    public static class AggregationEngine
    {
        public static ReportAggregationData Aggregate(
            List<Dictionary<string, object>> records,
            string groupByField,
            string aggregateFunction,
            string valueField)
        {
            if (records == null || records.Count == 0)
            {
                return new ReportAggregationData();
            }

            var groups = records
                .GroupBy(r => GetFieldValue(r, groupByField)?.ToString() ?? "(empty)")
                .ToList();

            var result = new ReportAggregationData
            {
                Columns = new List<ReportColumnDefinition>
                {
                    new ReportColumnDefinition { Header = groupByField, DataPath = groupByField, Format = "text", Width = 20 },
                    new ReportColumnDefinition { Header = $"{aggregateFunction}({valueField})", DataPath = valueField, Format = "number", Width = 15 }
                }
            };

            foreach (var group in groups)
            {
                var values = group
                    .Select(r => GetNumericValue(r, valueField))
                    .Where(v => v.HasValue)
                    .Select(v => v.Value)
                    .ToList();

                double aggregatedValue = ComputeAggregate(aggregateFunction, values);

                result.Rows.Add(new List<object> { group.Key, aggregatedValue });
            }

            return result;
        }

        private static double ComputeAggregate(string function, List<double> values)
        {
            if (values.Count == 0) return 0;

            return (function?.ToLowerInvariant()) switch
            {
                "sum" => values.Sum(),
                "avg" or "average" => values.Average(),
                "min" => values.Min(),
                "max" => values.Max(),
                "count" => values.Count,
                _ => values.Sum()
            };
        }

        private static object GetFieldValue(Dictionary<string, object> record, string field)
        {
            if (record.TryGetValue(field, out var value))
            {
                return value;
            }
            return null;
        }

        private static double? GetNumericValue(Dictionary<string, object> record, string field)
        {
            var value = GetFieldValue(record, field);
            if (value == null) return null;

            if (value is double d) return d;
            if (value is decimal dec) return (double)dec;
            if (value is int i) return i;
            if (value is long l) return l;
            if (value is float f) return f;

            if (double.TryParse(value.ToString(), out var parsed))
            {
                return parsed;
            }
            return null;
        }
    }
}
