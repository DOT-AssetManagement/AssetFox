using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Query;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AssetFox.Core.UniversalReporting.Components
{
    public class WorkSummaryDataSource : ComponentDataSourceBase, IComponentDataSource
    {
        public WorkSummaryDataSource(UnitOfDataPersistenceWork unitOfWork, CriteriaSqlEmitter criteriaEmitter)
            : base(unitOfWork, criteriaEmitter)
        {
        }

        public string ComponentType => "workSummary";

        public ComponentQuery Build(
            ReportExecutionContext executionContext,
            ViewerDataRequest request,
            int runId,
            AttributeLookup attributes) => GetWorkSummaryData(executionContext, request, runId);

        #region Work Summary

        private ComponentQuery GetWorkSummaryData(ReportExecutionContext executionContext, ViewerDataRequest request, int runId)
        {
            var component = executionContext.Component;
            var config = component.Config ?? new Dictionary<string, object>();
            var groupByAttribute = GetConfigString(config, "groupByAttribute", "");
            var aggregateField = GetConfigString(config, "aggregateField", "count");
            var aggregateFunction = GetConfigString(config, "aggregateFunction", "count");
            var includePercentages = GetConfigBool(config, "includePercentages");
            var groupByCalculatedFieldId = CalculatedFieldId(groupByAttribute);
            var aggregateCalculatedFieldId = CalculatedFieldId(aggregateField);
            var groupHeader = groupByCalculatedFieldId == null
                ? groupByAttribute
                : executionContext.CalculatedFields.FirstOrDefault(field =>
                    string.Equals(field.Id, groupByCalculatedFieldId, StringComparison.OrdinalIgnoreCase))?.Name ?? "Group";

            var calculatedAggregate = aggregateCalculatedFieldId == null
                ? null
                : executionContext.CalculatedFields.FirstOrDefault(field =>
                    string.Equals(field.Id, aggregateCalculatedFieldId, StringComparison.OrdinalIgnoreCase));
            (string valueHeader, string valueFormat) = calculatedAggregate == null
                ? WorkSummaryHeaderResolver.Resolve(aggregateField, aggregateFunction)
                : ($"{aggregateFunction} {calculatedAggregate.Name}", "number");

            var context = _unitOfWork.Context;

            // Get the year list for wide-format pivot columns
            var years = context.SimulationYearDetail
                .AsNoTracking()
                .Where(y => y.RunId == runId)
                .OrderBy(y => y.Year)
                .Select(y => y.Year)
                .ToList();

            // Build wide-format columns: group key + one value column per year
            var columns = new List<ViewerColumnDTO>
            {
                new ViewerColumnDTO
                {
                    Key = "groupValue",
                    Header = string.IsNullOrEmpty(groupHeader) ? "Group" : groupHeader,
                    Format = "text",
                    Filterable = true,
                    Sortable = true
                }
            };
            foreach (var year in years)
            {
                columns.Add(new ViewerColumnDTO
                {
                    Key = $"y{year}",
                    Header = $"{year} {valueHeader}",
                    Format = valueFormat,
                    Filterable = false,
                    Sortable = false
                });
                if (includePercentages)
                {
                    columns.Add(new ViewerColumnDTO
                    {
                        Key = $"p{year}",
                        Header = $"{year} %",
                        Format = "percent",
                        Filterable = false,
                        Sortable = false
                    });
                }
            }

            return new ComponentQuery(
                string.Empty,
                Array.Empty<SqlParameter>(),
                columns.Select(ToViewerColumn).ToList(),
                RequiresInMemoryPagination: true,
                cancellationToken => ReadRowsAsync(
                    request,
                    runId,
                    columns,
                    years,
                    groupByAttribute,
                    aggregateField,
                    aggregateFunction,
                    includePercentages,
                    executionContext,
                    cancellationToken));
        }

        private async IAsyncEnumerable<List<object>> ReadRowsAsync(
            ViewerDataRequest request,
            int runId,
            List<ViewerColumnDTO> columns,
            List<int> years,
            string groupByAttribute,
            string aggregateField,
            string aggregateFunction,
            bool includePercentages,
            ReportExecutionContext executionContext,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var rows = new List<List<object>>();
            // Build the groupBy expression using AggregatedResult (which has ALL attributes including static ones)
            // or COALESCE with AssetDetailValueIntId for per-year attributes
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@RunId", runId)
            };
            var groupByCalculatedFieldId = CalculatedFieldId(groupByAttribute);
            var aggregateCalculatedFieldId = CalculatedFieldId(aggregateField);
            var calculatedFields = BuildCalculatedFieldProjection(
                executionContext,
                new[] { groupByCalculatedFieldId, aggregateCalculatedFieldId }
                    .Where(id => !string.IsNullOrWhiteSpace(id))!,
                ReportCalculatedFieldRowScope.Yearly,
                parameters,
                "workSummary");
            var groupCalculatedField = calculatedFields.FindById(groupByCalculatedFieldId);
            var aggregateCalculatedField = calculatedFields.FindById(aggregateCalculatedFieldId);
            if (aggregateCalculatedField != null
                && !string.Equals(aggregateCalculatedField.ResultType, "number", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Work Summary aggregate calculated field '{aggregateCalculatedField.Name}' must be numeric.");
            }

            // Determine whether cost needs to be aggregated
            bool needsCostInCte = aggregateCalculatedField == null
                && aggregateField.Equals("cost", StringComparison.OrdinalIgnoreCase)
                && !aggregateFunction.Equals("count", StringComparison.OrdinalIgnoreCase);

            string aggSelect;
            string groupAttrCtes = "";
            string groupAttrJoins = "";
            string groupValueSelect;

            if (groupCalculatedField != null)
            {
                groupValueSelect = $"CAST(wscfv.[{groupCalculatedField.ColumnKey}] AS NVARCHAR(4000)) AS GroupValue";
            }
            else if (string.IsNullOrEmpty(groupByAttribute))
            {
                groupValueSelect = "'All' AS GroupValue";
            }
            else
            {
                var attr = _unitOfWork.AttributeRepo.GetSingleByName(groupByAttribute);
                if (attr == null)
                {
                    yield break;
                }

                parameters.Add(new SqlParameter("@GroupAttrId", attr.Id));

                // One scan of per-year EAV for the group-by attribute across all treated asset rows
                // One scan of summary EAV as fallback for static attributes not stored per-year
                string eavValueCol = attr.Type == "NUMBER" ? "NumericValue" : "TextValue";
                string summaryValueExpr = attr.Type == "NUMBER"
                    ? "CAST(gs.GroupValue AS NVARCHAR(200))"
                    : "gs.GroupValue";
                string perYearValueExpr = attr.Type == "NUMBER"
                    ? "CAST(gy.GroupValue AS NVARCHAR(200))"
                    : "gy.GroupValue";

                groupAttrCtes = $@",
GroupAttrPerYear AS (
    SELECT adv.AssetDetailId, adv.{eavValueCol} AS GroupValue
    FROM dbo.AssetDetailValueIntId adv
    WHERE adv.RunId = @RunId
      AND adv.AttributeId = @GroupAttrId
      AND adv.AssetDetailId IN (SELECT Id FROM AssetBase)
),
GroupAttrSummary AS (
    SELECT asd.MaintainableAssetId, asdv.{eavValueCol} AS GroupValue
    FROM dbo.AssetSummaryDetailValueIntId asdv
    INNER JOIN dbo.AssetSummaryDetail asd
        ON asd.RunId = @RunId AND asd.Id = asdv.AssetSummaryDetailId
    WHERE asdv.RunId = @RunId
      AND asdv.AttributeId = @GroupAttrId
      AND asd.MaintainableAssetId IN (SELECT MaintainableAssetId FROM AssetBase)
)";
                groupAttrJoins = @"
    LEFT JOIN GroupAttrPerYear gy  ON gy.AssetDetailId       = ab.Id
    LEFT JOIN GroupAttrSummary gs  ON gs.MaintainableAssetId = ab.MaintainableAssetId";
                groupValueSelect = $"COALESCE({perYearValueExpr}, {summaryValueExpr}) AS GroupValue";
            }

            // Cost CTE: one scan of TCD→FCO→Alloc for all treated asset rows in AssetBase
            string costCte = needsCostInCte ? @",
CostData AS (
    SELECT tcd.AssetDetailId,
        SUM(CAST(alloc.AllocatedAmount AS FLOAT)) AS TreatmentCost
    FROM dbo.TreatmentConsiderationDetail tcd
    INNER JOIN dbo.FundingCalculationOutput fco
        ON fco.RunId = tcd.RunId AND fco.TreatmentConsiderationDetailId = tcd.Id
    INNER JOIN dbo.Allocation alloc
        ON alloc.RunId = fco.RunId AND alloc.FundingCalculationOutputId = fco.Id
    WHERE tcd.RunId = @RunId
      AND tcd.AssetDetailId IN (SELECT Id FROM AssetBase)
    GROUP BY tcd.AssetDetailId
)" : "";

            string costJoin = needsCostInCte ? "\n    LEFT JOIN CostData cd ON cd.AssetDetailId = ab.Id" : "";
            string costColumn = needsCostInCte ? ", ISNULL(cd.TreatmentCost, 0) AS TreatmentCost" : "";

            if (aggregateCalculatedField != null)
            {
                string sqlAggFunc = aggregateFunction.ToUpperInvariant() switch
                {
                    "AVG" => "AVG",
                    "MIN" => "MIN",
                    "MAX" => "MAX",
                    "COUNT" => "COUNT",
                    _ => "SUM"
                };
                aggSelect = $"{sqlAggFunc}(adata.CalculatedValue)";
            }
            else if (needsCostInCte)
            {
                string sqlAggFunc = aggregateFunction.ToUpperInvariant() switch
                {
                    "SUM" => "SUM",
                    "AVG" => "AVG",
                    "MIN" => "MIN",
                    "MAX" => "MAX",
                    _ => "SUM"
                };
                aggSelect = $"{sqlAggFunc}(adata.TreatmentCost)";
            }
            else
            {
                aggSelect = "COUNT(*)";
            }

            var calculatedFieldCte = calculatedFields.IsEmpty ? "" : $",\n{calculatedFields.CteSql}";
            var calculatedFieldJoin = calculatedFields.IsEmpty
                ? ""
                : $"\n    {calculatedFields.Join("ab", "Id", "wscfv")}";
            var calculatedValueColumn = aggregateCalculatedField == null
                ? ""
                : $", wscfv.[{aggregateCalculatedField.ColumnKey}] AS CalculatedValue";

            // AssetBase CTE: the full treated-asset-year set with no correlated lookups
            // GroupAttr/Cost CTEs scan EAV once using IN (SELECT ... FROM AssetBase)
            // AssetData joins them all set-based
            var sql = $@"
;WITH AssetBase AS (
    SELECT ad.Id, ad.MaintainableAssetId, syd.Year
    FROM dbo.AssetDetail ad
    INNER JOIN dbo.SimulationYearDetail syd ON syd.RunId = @RunId AND syd.Id = ad.SimulationYearDetailId
    WHERE ad.RunId = @RunId
    AND ad.AppliedTreatment IS NOT NULL AND ad.AppliedTreatment <> 'No Treatment'
){calculatedFieldCte}{groupAttrCtes}{costCte},
AssetData AS (
    SELECT ab.Id, ab.MaintainableAssetId, ab.Year,
        {groupValueSelect}{costColumn}{calculatedValueColumn}
    FROM AssetBase ab{calculatedFieldJoin}{groupAttrJoins}{costJoin}
)
SELECT ISNULL(adata.GroupValue, '(None)') AS GroupValue, adata.Year, {aggSelect} AS AggValue
FROM AssetData adata
GROUP BY adata.GroupValue, adata.Year
ORDER BY adata.GroupValue, adata.Year";

            await using var connection = new SqlConnection(_unitOfWork.Context.Database.GetDbConnection().ConnectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 120;
            foreach (var p in parameters)
                command.Parameters.Add(new SqlParameter(p.ParameterName, p.Value ?? DBNull.Value));

            // Read long-format rows from SQL, then pivot to wide format
            // SQL returns: (GroupValue, Year, AggValue) per group×year
            var yearIndexMap = new Dictionary<int, int>();
            for (int yi = 0; yi < years.Count; yi++)
                yearIndexMap[years[yi]] = yi;

            var pivotData = new Dictionary<string, object[]>(StringComparer.OrdinalIgnoreCase);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var groupValue = reader.IsDBNull(0) ? "(None)" : reader.GetString(0);
                var year = reader.GetInt32(1);
                var aggValue = reader.GetValue(2);

                // Convert to appropriate type
                object displayValue;
                if (aggValue is decimal d) displayValue = (double)d;
                else if (aggValue is int i) displayValue = i;
                else if (aggValue is long l) displayValue = l;
                else displayValue = Convert.ToDouble(aggValue);

                if (!pivotData.TryGetValue(groupValue, out var yearValues))
                {
                    yearValues = new object[years.Count];
                    pivotData[groupValue] = yearValues;
                }

                if (yearIndexMap.TryGetValue(year, out int idx))
                    yearValues[idx] = displayValue;
            }

            // Build wide-format rows: [groupValue, y2025_val, [y2025_pct,] y2026_val, [y2026_pct,] ...]
            // When includePercentages is on, each row's per-year value is divided by the year's
            // grand total (across all groups) so columns sum to 100%.
            var yearTotals = new double[years.Count];
            if (includePercentages)
            {
                foreach (var kvp in pivotData)
                {
                    for (var yi = 0; yi < kvp.Value.Length; yi++)
                    {
                        var v = kvp.Value[yi];
                        if (v is double d) yearTotals[yi] += d;
                        else if (v is int i) yearTotals[yi] += i;
                        else if (v is long l) yearTotals[yi] += l;
                    }
                }
            }

            foreach (var kvp in pivotData.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                var row = new List<object> { kvp.Key };
                for (var yi = 0; yi < kvp.Value.Length; yi++)
                {
                    var val = kvp.Value[yi] ?? (object)0;
                    row.Add(val);
                    if (includePercentages)
                    {
                        double numericVal = 0;
                        if (val is double d) numericVal = d;
                        else if (val is int i) numericVal = i;
                        else if (val is long l) numericVal = l;
                        var pct = yearTotals[yi] > 0 ? (numericVal / yearTotals[yi]) * 100 : 0d;
                        row.Add(pct);
                    }
                }
                rows.Add(row);
            }

            foreach (var row in ApplyInMemoryFilterAndSort(columns, rows, request))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return row;
            }
        }

        #endregion

        private static string? CalculatedFieldId(string value)
        {
            const string prefix = "calculatedField:";
            return value?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == true
                ? value[prefix.Length..]
                : null;
        }


    }
}
