using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.Reporting.Models.FlexibleReport;
using Newtonsoft.Json.Linq;

namespace AssetFox.Core.Reporting.Services.FlexibleReport
{
    public class DataExtractor
    {
        // ============================================================
        // Component-aware assembly methods
        // ============================================================

        public ReportTableData AssembleAssetDetailData(SimulationOutput output, Dictionary<string, object> config)
        {
            var timeScope = GetConfigString(config, "timeScope", "initial");
            var attributeColumns = GetConfigColumnDefs(config, "attributeColumns");
            var simulationColumns = GetConfigSimulationColumnDefs(config, "simulationColumns");
            var sorting = GetConfigSorting(config);
            var showTotals = GetConfigBool(config, "showTotals");

            List<Dictionary<string, object>> records;
            if (timeScope == "initial")
            {
                records = ExtractInitialAssets(output);
            }
            else if (timeScope == "year")
            {
                var selectedYear = GetConfigInt(config, "selectedYear", 0);
                records = ExtractYearlyAssets(output)
                    .Where(r => r.ContainsKey("year") && Convert.ToInt32(r["year"]) == selectedYear)
                    .ToList();
            }
            else // allYears
            {
                records = ExtractYearlyAssets(output);
            }

            return BuildTableFromRecords(records, attributeColumns, simulationColumns, sorting);
        }

        public ReportTableData AssembleTreatmentTimelineData(SimulationOutput output, Dictionary<string, object> config)
        {
            var attributeColumns = GetConfigColumnDefs(config, "attributeColumns");
            var yearDataFields = GetConfigYearFields(config, "yearDataFields");
            var sorting = GetConfigSorting(config);
            var showNoTreatment = GetConfigBool(config, "showNoTreatment");

            var yearlyRecords = ExtractYearlyAssets(output);
            if (!showNoTreatment)
            {
                yearlyRecords = yearlyRecords
                    .Where(r => !string.Equals(
                        r.TryGetValue("appliedTreatment", out var treatment) ? treatment?.ToString() : null,
                        "No Treatment",
                        StringComparison.Ordinal))
                    .ToList();
            }

            // Group by asset
            var assetGroups = yearlyRecords
                .GroupBy(r => r.ContainsKey("assetId") ? r["assetId"]?.ToString() ?? "" : "")
                .ToList();

            var years = output.Years.Select(y => y.Year).OrderBy(y => y).ToList();

            // Build column definitions: auto asset-key column + attribute cols + per-year columns
            var columns = new List<ReportColumnDefinition>
            {
                new ReportColumnDefinition { Header = "Asset ID", DataPath = "assetName", Format = "text", Width = 18 }
            };

            foreach (var col in attributeColumns)
                columns.Add(new ReportColumnDefinition { Header = col.Header, DataPath = col.DataPath, Format = col.Format, Width = col.Width });

            foreach (var year in years)
            {
                foreach (var field in yearDataFields)
                {
                    var header = $"{year} {field.Header}";
                    columns.Add(new ReportColumnDefinition { Header = header, DataPath = $"year_{year}_{field.DataPath}", Format = field.Format, Width = 12 });
                }
            }

            // Build rows
            var rows = new List<List<object>>();
            foreach (var group in assetGroups)
            {
                var row = new List<object>();

                // Use first record for asset key + attribute columns
                var firstRecord = group.First();
                row.Add(ResolveDataPath(firstRecord, "assetName"));
                foreach (var col in attributeColumns)
                    row.Add(ResolveDataPath(firstRecord, col.DataPath));

                // Per-year data
                var recordsByYear = group.ToDictionary(
                    r => r.ContainsKey("year") ? Convert.ToInt32(r["year"]) : 0,
                    r => r);

                foreach (var year in years)
                {
                    recordsByYear.TryGetValue(year, out var yearRecord);
                    foreach (var field in yearDataFields)
                    {
                        row.Add(yearRecord != null ? ResolveDataPath(yearRecord, field.DataPath) : null);
                    }
                }

                rows.Add(row);
            }

            // Apply sorting
            if (sorting != null && !string.IsNullOrWhiteSpace(sorting.Field))
            {
                var sortColIndex = columns.FindIndex(c =>
                    string.Equals(c.DataPath, sorting.Field, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Header, sorting.Field, StringComparison.OrdinalIgnoreCase));

                if (sortColIndex >= 0)
                {
                    var ascending = string.Equals(sorting.Direction, "asc", StringComparison.OrdinalIgnoreCase);
                    rows = ascending
                        ? rows.OrderBy(r => GetSortableObjectValue(r.ElementAtOrDefault(sortColIndex))).ToList()
                        : rows.OrderByDescending(r => GetSortableObjectValue(r.ElementAtOrDefault(sortColIndex))).ToList();
                }
            }

            return new ReportTableData
            {
                Columns = columns,
                Rows = rows,
                TotalRowCount = rows.Count
            };
        }

        public ReportAggregationData AssembleWorkSummaryData(SimulationOutput output, Dictionary<string, object> config)
        {
            var groupByAttribute = GetConfigString(config, "groupByAttribute", "");
            var aggregateField = GetConfigString(config, "aggregateField", "cost");
            var aggregateFunction = GetConfigString(config, "aggregateFunction", "sum");

            var records = ExtractYearlyAssets(output);

            // Map aggregate field to actual data path
            var valuePath = MapAggregateField(aggregateField);
            var groupPath = $"text.{groupByAttribute}";

            // Try numeric path if text doesn't exist
            if (records.Count > 0 && !records[0].ContainsKey(groupPath))
            {
                groupPath = $"numeric.{groupByAttribute}";
                if (records.Count > 0 && !records[0].ContainsKey(groupPath))
                {
                    groupPath = groupByAttribute;
                }
            }

            return AggregationEngine.Aggregate(records, groupPath, aggregateFunction, valuePath);
        }

        public ReportTableData AssembleBudgetSummaryData(SimulationOutput output, Dictionary<string, object> config)
        {
            var showAvailable = GetConfigBool(config, "showAvailableFunding", true);
            var showSpent = GetConfigBool(config, "showSpentAmount", true);
            var showRemaining = GetConfigBool(config, "showRemainingAmount", true);

            var budgetRecords = ExtractYearlyBudgets(output);

            // Pivot: rows = budget names, columns = years
            var budgetNames = budgetRecords
                .Select(r => r["budgetName"]?.ToString() ?? "")
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var years = output.Years.Select(y => y.Year).OrderBy(y => y).ToList();

            var columns = new List<ReportColumnDefinition>
            {
                new ReportColumnDefinition { Header = "Budget", DataPath = "budgetName", Format = "text", Width = 20 }
            };

            foreach (var year in years)
            {
                if (showAvailable)
                    columns.Add(new ReportColumnDefinition { Header = $"{year} Available", DataPath = $"available_{year}", Format = "currency", Width = 15 });
                if (showSpent)
                    columns.Add(new ReportColumnDefinition { Header = $"{year} Spent", DataPath = $"spent_{year}", Format = "currency", Width = 15 });
                if (showRemaining)
                    columns.Add(new ReportColumnDefinition { Header = $"{year} Remaining", DataPath = $"remaining_{year}", Format = "currency", Width = 15 });
            }

            // Calculate spent amounts from yearly assets
            var yearlyAssets = ExtractYearlyAssets(output);
            var spentByBudgetYear = yearlyAssets
                .Where(r => r.ContainsKey("budget") && r.ContainsKey("cost"))
                .GroupBy(r => new
                {
                    Budget = r["budget"]?.ToString() ?? "",
                    Year = r.ContainsKey("year") ? Convert.ToInt32(r["year"]) : 0
                })
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(r =>
                    {
                        var cost = r.ContainsKey("cost") ? r["cost"] : null;
                        if (cost is double d) return d;
                        if (cost is decimal dec) return (double)dec;
                        return 0.0;
                    }));

            var rows = new List<List<object>>();
            foreach (var budgetName in budgetNames)
            {
                var row = new List<object> { budgetName };
                foreach (var year in years)
                {
                    var budgetRecord = budgetRecords.FirstOrDefault(r =>
                        r["budgetName"]?.ToString() == budgetName &&
                        Convert.ToInt32(r["year"]) == year);

                    var available = budgetRecord != null && budgetRecord.ContainsKey("availableFunding")
                        ? Convert.ToDouble(budgetRecord["availableFunding"]) : 0.0;

                    var key = new { Budget = budgetName, Year = year };
                    var spent = spentByBudgetYear.ContainsKey(key) ? spentByBudgetYear[key] : 0.0;

                    if (showAvailable) row.Add(available);
                    if (showSpent) row.Add(spent);
                    if (showRemaining) row.Add(available - spent);
                }
                rows.Add(row);
            }

            return new ReportTableData
            {
                Columns = columns,
                Rows = rows,
                TotalRowCount = rows.Count
            };
        }

        public ReportTableData AssembleConditionTrackingData(SimulationOutput output, Dictionary<string, object> config)
        {
            var conditionAttributes = GetConfigStringList(config, "conditionAttributes");
            var attributeColumns = GetConfigColumnDefs(config, "attributeColumns");
            var showInitialState = GetConfigBool(config, "showInitialState", true);

            var years = output.Years.Select(y => y.Year).OrderBy(y => y).ToList();

            // Build columns: auto asset-key column + attribute cols + per-year condition cols
            var columns = new List<ReportColumnDefinition>
            {
                new ReportColumnDefinition { Header = "Asset ID", DataPath = "assetName", Format = "text", Width = 18 }
            };
            foreach (var col in attributeColumns)
                columns.Add(new ReportColumnDefinition { Header = col.Header, DataPath = col.DataPath, Format = col.Format, Width = col.Width });

            if (showInitialState)
            {
                foreach (var attr in conditionAttributes)
                    columns.Add(new ReportColumnDefinition { Header = $"Initial {attr}", DataPath = $"initial_{attr}", Format = "number", Width = 12 });
            }

            foreach (var year in years)
            {
                foreach (var attr in conditionAttributes)
                    columns.Add(new ReportColumnDefinition { Header = $"{year} {attr}", DataPath = $"year_{year}_{attr}", Format = "number", Width = 12 });
            }

            // Build rows: one per asset
            var initialAssets = ExtractInitialAssets(output);
            var yearlyAssets = ExtractYearlyAssets(output);
            var assetGroups = yearlyAssets
                .GroupBy(r => r.ContainsKey("assetId") ? r["assetId"]?.ToString() ?? "" : "")
                .ToList();

            var initialByAsset = initialAssets.ToDictionary(
                r => r.ContainsKey("assetId") ? r["assetId"]?.ToString() ?? "" : "",
                r => r);

            var rows = new List<List<object>>();
            foreach (var group in assetGroups)
            {
                var row = new List<object>();
                var firstRecord = group.First();

                row.Add(ResolveDataPath(firstRecord, "assetName"));
                foreach (var col in attributeColumns)
                    row.Add(ResolveDataPath(firstRecord, col.DataPath));

                if (showInitialState)
                {
                    initialByAsset.TryGetValue(group.Key, out var initialRecord);
                    foreach (var attr in conditionAttributes)
                        row.Add(initialRecord != null ? ResolveDataPath(initialRecord, $"numeric.{attr}") : null);
                }

                var recordsByYear = group.ToDictionary(
                    r => r.ContainsKey("year") ? Convert.ToInt32(r["year"]) : 0,
                    r => r);

                foreach (var year in years)
                {
                    recordsByYear.TryGetValue(year, out var yearRecord);
                    foreach (var attr in conditionAttributes)
                        row.Add(yearRecord != null ? ResolveDataPath(yearRecord, $"numeric.{attr}") : null);
                }

                rows.Add(row);
            }

            return new ReportTableData
            {
                Columns = columns,
                Rows = rows,
                TotalRowCount = rows.Count
            };
        }

        public ReportTableData AssembleFundedTreatmentListData(SimulationOutput output, Dictionary<string, object> config)
        {
            var attributeColumns = GetConfigColumnDefs(config, "columns");
            var simulationColumns = GetConfigSimulationColumnDefs(config, "simulationColumns");
            var sorting = GetConfigSorting(config);

            // Get yearly assets filtered to funded treatments only
            var records = ExtractYearlyAssets(output)
                .Where(r =>
                {
                    if (!r.ContainsKey("appliedTreatment")) return false;
                    var treatment = r["appliedTreatment"]?.ToString();
                    return !string.IsNullOrEmpty(treatment) &&
                           !string.Equals(treatment, "No Treatment", StringComparison.OrdinalIgnoreCase);
                })
                .ToList();

            return BuildTableFromRecords(records, attributeColumns, simulationColumns, sorting);
        }

        public ReportTableData AssembleGoalTrackingData(SimulationOutput output, Dictionary<string, object> config)
        {
            var goalType = GetConfigString(config, "goalType", "target");
            var showGoalName = GetConfigBool(config, "showGoalName", true);
            var showActualVsTarget = GetConfigBool(config, "showActualVsTarget", true);
            var showMetStatus = GetConfigBool(config, "showMetStatus", true);

            var records = goalType == "deficient"
                ? ExtractYearlyDeficientGoals(output)
                : ExtractYearlyTargetGoals(output);

            var columns = new List<ReportColumnDefinition>
            {
                new ReportColumnDefinition { Header = "Year", DataPath = "year", Format = "number", Width = 10 }
            };

            if (showGoalName)
                columns.Add(new ReportColumnDefinition { Header = "Goal Name", DataPath = "goalName", Format = "text", Width = 20 });

            columns.Add(new ReportColumnDefinition { Header = "Attribute", DataPath = "attributeName", Format = "text", Width = 15 });

            if (showActualVsTarget)
            {
                if (goalType == "deficient")
                {
                    columns.Add(new ReportColumnDefinition { Header = "Actual Deficient %", DataPath = "actualDeficientPercentage", Format = "percent", Width = 15 });
                    columns.Add(new ReportColumnDefinition { Header = "Allowed Deficient %", DataPath = "allowedDeficientPercentage", Format = "percent", Width = 15 });
                }
                else
                {
                    columns.Add(new ReportColumnDefinition { Header = "Actual Value", DataPath = "actualValue", Format = "number", Width = 15 });
                    columns.Add(new ReportColumnDefinition { Header = "Target Value", DataPath = "targetValue", Format = "number", Width = 15 });
                }
            }

            if (showMetStatus)
                columns.Add(new ReportColumnDefinition { Header = "Goal Met", DataPath = "goalIsMet", Format = "text", Width = 10 });

            var rows = new List<List<object>>();
            foreach (var record in records)
            {
                var row = columns.Select(c => ResolveDataPath(record, c.DataPath)).ToList();
                rows.Add(row);
            }

            return new ReportTableData
            {
                Columns = columns,
                Rows = rows,
                TotalRowCount = rows.Count
            };
        }

        public ReportTextData AssembleTextBlockData(Dictionary<string, object> config)
        {
            return new ReportTextData
            {
                Content = GetConfigString(config, "content", ""),
                Style = GetConfigString(config, "style", "body")
            };
        }

        // ============================================================
        // Core data resolution
        // ============================================================

        public object ResolveDataPath(Dictionary<string, object> record, string dataPath)
        {
            if (record == null || string.IsNullOrWhiteSpace(dataPath)) return null;

            if (record.TryGetValue(dataPath, out var directValue))
                return directValue;

            if (dataPath.StartsWith("numeric.", StringComparison.OrdinalIgnoreCase))
            {
                var attrName = dataPath.Substring(8);
                if (record.TryGetValue($"numeric.{attrName}", out var numVal))
                    return numVal;
            }
            else if (dataPath.StartsWith("text.", StringComparison.OrdinalIgnoreCase))
            {
                var attrName = dataPath.Substring(5);
                if (record.TryGetValue($"text.{attrName}", out var txtVal))
                    return txtVal;
            }

            return null;
        }

        // ============================================================
        // Config deserialization helpers
        // ============================================================

        private static string GetConfigString(Dictionary<string, object> config, string key, string defaultValue = "")
        {
            if (config != null && config.TryGetValue(key, out var val) && val != null)
                return val.ToString();
            return defaultValue;
        }

        private static int GetConfigInt(Dictionary<string, object> config, string key, int defaultValue = 0)
        {
            if (config != null && config.TryGetValue(key, out var val) && val != null)
            {
                if (val is int i) return i;
                if (val is long l) return (int)l;
                if (int.TryParse(val.ToString(), out var parsed)) return parsed;
            }
            return defaultValue;
        }

        private static bool GetConfigBool(Dictionary<string, object> config, string key, bool defaultValue = false)
        {
            if (config != null && config.TryGetValue(key, out var val) && val != null)
            {
                if (val is bool b) return b;
                if (bool.TryParse(val.ToString(), out var parsed)) return parsed;
            }
            return defaultValue;
        }

        private static List<string> GetConfigStringList(Dictionary<string, object> config, string key)
        {
            if (config != null && config.TryGetValue(key, out var val) && val != null)
            {
                if (val is JArray jArr)
                    return jArr.Select(t => t.ToString()).ToList();
                if (val is List<object> list)
                    return list.Select(o => o?.ToString() ?? "").ToList();
            }
            return new List<string>();
        }

        private static SimpleColumnDef[] GetConfigColumnDefs(Dictionary<string, object> config, string key)
        {
            if (config != null && config.TryGetValue(key, out var val) && val != null)
            {
                if (val is JArray jArr)
                {
                    return jArr.Select(t => new SimpleColumnDef
                    {
                        Header = t["displayHeader"]?.ToString() ?? t["attributeName"]?.ToString() ?? "",
                        DataPath = ResolveColumnDataPath(t["attributeName"]?.ToString()),
                        Format = t["format"]?.ToString() ?? "text",
                        Width = t["width"]?.Value<double>() ?? 12
                    }).ToArray();
                }
            }
            return Array.Empty<SimpleColumnDef>();
        }

        private static SimpleColumnDef[] GetConfigSimulationColumnDefs(Dictionary<string, object> config, string key)
        {
            if (config != null && config.TryGetValue(key, out var val) && val != null)
            {
                if (val is JArray jArr)
                {
                    return jArr.Select(t =>
                    {
                        var field = t["field"]?.ToString() ?? "";
                        return new SimpleColumnDef
                        {
                            Header = t["displayHeader"]?.ToString() ?? field,
                            DataPath = MapSimulationField(field),
                            Format = t["format"]?.ToString() ?? "text",
                            Width = 12
                        };
                    }).ToArray();
                }
            }
            return Array.Empty<SimpleColumnDef>();
        }

        private static SimpleYearFieldDef[] GetConfigYearFields(Dictionary<string, object> config, string key)
        {
            if (config != null && config.TryGetValue(key, out var val) && val != null)
            {
                if (val is JArray jArr)
                {
                    return jArr.Select(t =>
                    {
                        var type = t["type"]?.ToString() ?? "treatment";
                        var attrName = t["attributeName"]?.ToString();
                        return new SimpleYearFieldDef
                        {
                            Header = t["displayHeader"]?.ToString() ?? type,
                            DataPath = type == "treatment" ? "appliedTreatment"
                                     : type == "cost" ? "cost"
                                     : $"numeric.{attrName}",
                            Format = t["format"]?.ToString() ?? "text"
                        };
                    }).ToArray();
                }
            }
            return Array.Empty<SimpleYearFieldDef>();
        }

        private static SortingConfig GetConfigSorting(Dictionary<string, object> config)
        {
            if (config != null && config.TryGetValue("sorting", out var val) && val != null)
            {
                if (val is JObject jObj)
                {
                    return new SortingConfig
                    {
                        Field = jObj["field"]?.ToString() ?? "",
                        Direction = jObj["direction"]?.ToString() ?? "asc"
                    };
                }
            }
            return null;
        }

        private static string ResolveColumnDataPath(string attributeName)
        {
            if (string.IsNullOrEmpty(attributeName)) return "";
            // Attribute names get a numeric prefix by default; text prefix is tried at resolution time
            return $"numeric.{attributeName}";
        }

        private static string MapSimulationField(string field)
        {
            return field switch
            {
                "year" => "year",
                "appliedTreatment" => "appliedTreatment",
                "treatmentCause" => "treatmentCause",
                "treatmentStatus" => "treatmentStatus",
                "cost" => "cost",
                "budget" => "budget",
                _ => field
            };
        }

        private static string MapAggregateField(string field)
        {
            return field switch
            {
                "cost" => "cost",
                "count" => "appliedTreatment",
                _ => field
            };
        }

        // ============================================================
        // Helper: build table from flat records
        // ============================================================

        private ReportTableData BuildTableFromRecords(
            List<Dictionary<string, object>> records,
            SimpleColumnDef[] attributeColumns,
            SimpleColumnDef[] simulationColumns,
            SortingConfig sorting)
        {
            var allColumns = new List<SimpleColumnDef>();
            allColumns.AddRange(attributeColumns);
            allColumns.AddRange(simulationColumns);

            var columns = allColumns.Select(c => new ReportColumnDefinition
            {
                Header = c.Header,
                DataPath = c.DataPath,
                Format = c.Format,
                Width = c.Width
            }).ToList();

            var sorted = ApplySorting(records, sorting);
            var rows = new List<List<object>>();

            foreach (var record in sorted)
            {
                var row = new List<object>();
                foreach (var col in allColumns)
                {
                    var value = ResolveDataPath(record, col.DataPath);
                    // Fallback: try text prefix if numeric not found
                    if (value == null && col.DataPath.StartsWith("numeric."))
                    {
                        var attrName = col.DataPath.Substring(8);
                        value = ResolveDataPath(record, $"text.{attrName}");
                    }
                    row.Add(value);
                }
                rows.Add(row);
            }

            return new ReportTableData
            {
                Columns = columns,
                Rows = rows,
                TotalRowCount = rows.Count
            };
        }

        // ============================================================
        // Internal helper types
        // ============================================================

        private class SimpleColumnDef
        {
            public string Header { get; set; }
            public string DataPath { get; set; }
            public string Format { get; set; }
            public double Width { get; set; }
        }

        private class SimpleYearFieldDef
        {
            public string Header { get; set; }
            public string DataPath { get; set; }
            public string Format { get; set; }
        }

        // ============================================================
        // Sorting
        // ============================================================

        private List<Dictionary<string, object>> ApplySorting(
            List<Dictionary<string, object>> records,
            SortingConfig sorting)
        {
            if (sorting == null || string.IsNullOrWhiteSpace(sorting.Field))
                return records;

            var ascending = string.Equals(sorting.Direction, "asc", StringComparison.OrdinalIgnoreCase);

            return ascending
                ? records.OrderBy(r => GetSortableValue(r, sorting.Field)).ToList()
                : records.OrderByDescending(r => GetSortableValue(r, sorting.Field)).ToList();
        }

        private object GetSortableValue(Dictionary<string, object> record, string field)
        {
            var value = ResolveDataPath(record, field);
            if (value is double || value is decimal || value is int || value is long || value is float)
                return Convert.ToDouble(value);
            return value?.ToString() ?? "";
        }

        private static object GetSortableObjectValue(object value)
        {
            if (value is double || value is decimal || value is int || value is long || value is float)
                return Convert.ToDouble(value);
            return value?.ToString() ?? "";
        }

        // ============================================================
        // Raw data extraction from SimulationOutput
        // ============================================================

        internal List<Dictionary<string, object>> ExtractInitialAssets(SimulationOutput output)
        {
            var records = new List<Dictionary<string, object>>();
            foreach (var asset in output.InitialAssetSummaries)
            {
                var record = new Dictionary<string, object>
                {
                    ["assetId"] = asset.AssetId,
                    ["assetName"] = asset.AssetName
                };

                foreach (var kvp in asset.ValuePerNumericAttribute)
                    record[$"numeric.{kvp.Key}"] = kvp.Value;
                foreach (var kvp in asset.ValuePerTextAttribute)
                    record[$"text.{kvp.Key}"] = kvp.Value;

                records.Add(record);
            }
            return records;
        }

        internal List<Dictionary<string, object>> ExtractYearlyAssets(SimulationOutput output)
        {
            var records = new List<Dictionary<string, object>>();
            foreach (var year in output.Years)
            {
                foreach (var asset in year.Assets)
                {
                    var record = new Dictionary<string, object>
                    {
                        ["year"] = year.Year,
                        ["assetId"] = asset.AssetId,
                        ["assetName"] = asset.AssetName,
                        ["appliedTreatment"] = asset.AppliedTreatment,
                        ["treatmentCause"] = asset.TreatmentCause.ToString(),
                        ["treatmentStatus"] = asset.TreatmentStatus.ToString(),
                        ["treatmentFundingIgnoresSpendingLimit"] = asset.TreatmentFundingIgnoresSpendingLimit
                    };

                    foreach (var kvp in asset.ValuePerNumericAttribute)
                        record[$"numeric.{kvp.Key}"] = kvp.Value;
                    foreach (var kvp in asset.ValuePerTextAttribute)
                        record[$"text.{kvp.Key}"] = kvp.Value;

                    // Extract cost and budget from treatment options
                    if (asset.TreatmentConsiderations != null && asset.TreatmentConsiderations.Count > 0)
                    {
                        var consideration = asset.TreatmentConsiderations.FirstOrDefault();
                        if (consideration != null)
                        {
                            record["budget"] = consideration.BudgetPriorityLevel;
                        }
                    }

                    if (asset.TreatmentOptions != null)
                    {
                        var appliedOption = asset.TreatmentOptions
                            .FirstOrDefault(o => string.Equals(o.TreatmentName, asset.AppliedTreatment, StringComparison.OrdinalIgnoreCase));
                        if (appliedOption != null)
                        {
                            record["cost"] = appliedOption.Cost;
                        }
                    }

                    records.Add(record);
                }
            }
            return records;
        }

        private List<Dictionary<string, object>> ExtractYearlyCondition(SimulationOutput output)
        {
            var records = new List<Dictionary<string, object>>();
            foreach (var year in output.Years)
            {
                records.Add(new Dictionary<string, object>
                {
                    ["year"] = year.Year,
                    ["conditionOfNetwork"] = year.ConditionOfNetwork
                });
            }
            return records;
        }

        internal List<Dictionary<string, object>> ExtractYearlyBudgets(SimulationOutput output)
        {
            var records = new List<Dictionary<string, object>>();
            foreach (var year in output.Years)
            {
                foreach (var budget in year.Budgets)
                {
                    records.Add(new Dictionary<string, object>
                    {
                        ["year"] = year.Year,
                        ["budgetName"] = budget.BudgetName,
                        ["availableFunding"] = (double)budget.AvailableFunding
                    });
                }
            }
            return records;
        }

        internal List<Dictionary<string, object>> ExtractYearlyDeficientGoals(SimulationOutput output)
        {
            var records = new List<Dictionary<string, object>>();
            foreach (var year in output.Years)
            {
                foreach (var goal in year.DeficientConditionGoals)
                {
                    records.Add(new Dictionary<string, object>
                    {
                        ["year"] = year.Year,
                        ["goalName"] = goal.GoalName,
                        ["attributeName"] = goal.AttributeName,
                        ["goalIsMet"] = goal.GoalIsMet,
                        ["actualDeficientPercentage"] = goal.ActualDeficientPercentage,
                        ["allowedDeficientPercentage"] = goal.AllowedDeficientPercentage,
                        ["deficientLimit"] = goal.DeficientLimit
                    });
                }
            }
            return records;
        }

        internal List<Dictionary<string, object>> ExtractYearlyTargetGoals(SimulationOutput output)
        {
            var records = new List<Dictionary<string, object>>();
            foreach (var year in output.Years)
            {
                foreach (var goal in year.TargetConditionGoals)
                {
                    records.Add(new Dictionary<string, object>
                    {
                        ["year"] = year.Year,
                        ["goalName"] = goal.GoalName,
                        ["attributeName"] = goal.AttributeName,
                        ["goalIsMet"] = goal.GoalIsMet,
                        ["actualValue"] = goal.ActualValue,
                        ["targetValue"] = goal.TargetValue
                    });
                }
            }
            return records;
        }

        private List<Dictionary<string, object>> ExtractTreatmentOptions(SimulationOutput output)
        {
            var records = new List<Dictionary<string, object>>();
            foreach (var year in output.Years)
            {
                foreach (var asset in year.Assets)
                {
                    foreach (var option in asset.TreatmentOptions)
                    {
                        records.Add(new Dictionary<string, object>
                        {
                            ["year"] = year.Year,
                            ["assetId"] = asset.AssetId,
                            ["assetName"] = asset.AssetName,
                            ["treatmentName"] = option.TreatmentName,
                            ["cost"] = option.Cost,
                            ["conditionChange"] = option.ConditionChange
                        });
                    }
                }
            }
            return records;
        }

        private List<Dictionary<string, object>> ExtractTreatmentRejections(SimulationOutput output)
        {
            var records = new List<Dictionary<string, object>>();
            foreach (var year in output.Years)
            {
                foreach (var asset in year.Assets)
                {
                    foreach (var rejection in asset.TreatmentRejections)
                    {
                        records.Add(new Dictionary<string, object>
                        {
                            ["year"] = year.Year,
                            ["assetId"] = asset.AssetId,
                            ["assetName"] = asset.AssetName,
                            ["treatmentName"] = rejection.TreatmentName,
                            ["rejectionReason"] = rejection.TreatmentRejectionReason.ToString(),
                            ["potentialConditionChange"] = rejection.PotentialConditionChange
                        });
                    }
                }
            }
            return records;
        }

        private List<Dictionary<string, object>> ExtractTreatmentConsiderations(SimulationOutput output)
        {
            var records = new List<Dictionary<string, object>>();
            foreach (var year in output.Years)
            {
                foreach (var asset in year.Assets)
                {
                    foreach (var consideration in asset.TreatmentConsiderations)
                    {
                        records.Add(new Dictionary<string, object>
                        {
                            ["year"] = year.Year,
                            ["assetId"] = asset.AssetId,
                            ["assetName"] = asset.AssetName,
                            ["treatmentName"] = consideration.TreatmentName,
                            ["budgetPriorityLevel"] = consideration.BudgetPriorityLevel
                        });
                    }
                }
            }
            return records;
        }

        private List<Dictionary<string, object>> ExtractTreatmentSchedulingCollisions(SimulationOutput output)
        {
            var records = new List<Dictionary<string, object>>();
            foreach (var year in output.Years)
            {
                foreach (var asset in year.Assets)
                {
                    foreach (var collision in asset.TreatmentSchedulingCollisions)
                    {
                        records.Add(new Dictionary<string, object>
                        {
                            ["year"] = year.Year,
                            ["assetId"] = asset.AssetId,
                            ["assetName"] = asset.AssetName,
                            ["collisionYear"] = collision.Year,
                            ["nameOfUnscheduledTreatment"] = collision.NameOfUnscheduledTreatment
                        });
                    }
                }
            }
            return records;
        }

        // Sorting config class (used internally)
        internal class SortingConfig
        {
            public string Field { get; set; }
            public string Direction { get; set; } = "asc";
        }
    }
}
