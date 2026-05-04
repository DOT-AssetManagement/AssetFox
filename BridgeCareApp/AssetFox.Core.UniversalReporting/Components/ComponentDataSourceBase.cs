using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Query;
using AssetFox.Core.UniversalReporting.Viewer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace AssetFox.Core.UniversalReporting.Components
{
    public abstract class ComponentDataSourceBase
    {
        protected readonly UnitOfDataPersistenceWork _unitOfWork;
        protected readonly CriteriaSqlEmitter _criteriaEmitter;

        // All known simulation fields (direct columns + computed fields)
        protected static readonly List<string> AssetDetailSimulationFields = new()
        {
            "year", "appliedTreatment", "treatmentCause", "treatmentStatus",
            "cost", "budget"
        };

        // Fields already hardcoded in the GetAssetYearlyData CTE (to prevent duplicate columns)
        protected static readonly HashSet<string> HardcodedYearlyFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "year", "appliedTreatment", "treatmentCause", "treatmentStatus",
            "cost", "budget"
        };

        // Fields already hardcoded in the GetFundedTreatmentListData CTE
        protected static readonly HashSet<string> HardcodedFundedTreatmentFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "year", "appliedTreatment", "treatmentCause", "treatmentStatus",
            "cost", "budget"
        };

        protected ComponentDataSourceBase(UnitOfDataPersistenceWork unitOfWork, CriteriaSqlEmitter criteriaEmitter)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _criteriaEmitter = criteriaEmitter ?? throw new ArgumentNullException(nameof(criteriaEmitter));
        }

        #region Helpers

        protected int ResolveRunId(Guid simulationId)
        {
            return _unitOfWork.Context.SimulationOutput
                .Where(so => so.SimulationId == simulationId)
                .Select(so => so.RunId)
                .FirstOrDefault();
        }

        /// <summary>
        /// Resolves the network's key attribute for a given simulation.
        /// The key attribute's AggregatedResult value serves as the asset identifier.
        /// </summary>
        protected KeyAttributeInfo ResolveKeyAttribute(Guid simulationId)
        {
            var context = _unitOfWork.Context;

            var keyAttributeId = context.Simulation
                .AsNoTracking()
                .Where(s => s.Id == simulationId)
                .Select(s => s.Network.KeyAttributeId)
                .FirstOrDefault();

            if (keyAttributeId == Guid.Empty)
                return new KeyAttributeInfo { Id = Guid.Empty, Name = "Asset ID", Type = "TEXT" };

            var attr = _unitOfWork.AttributeRepo.GetSingleById(keyAttributeId);
            if (attr == null)
                return new KeyAttributeInfo { Id = keyAttributeId, Name = "Asset ID", Type = "TEXT" };

            return new KeyAttributeInfo { Id = attr.Id, Name = attr.Name, Type = attr.Type };
        }

        /// <summary>
        /// Resolves the asset's key display name (LocationIdentifier) by joining to MaintainableAssetLocation.
        /// This is a 30k-row table with a unique index on MaintainableAssetId, making it orders of magnitude
        /// faster than the previous approach of correlated scalar subqueries into the 6M-row EAV table.
        /// MaintainableAssetLocation.LocationIdentifier contains the same value as the network's key attribute
        /// (populated during network creation/import from the LOCATION_IDENTIFIER data column).
        /// </summary>
        /// <param name="parentAlias">The FROM alias in the CTE that has MaintainableAssetId (e.g., "fa").</param>
        /// <returns>A SQL JOIN clause and SELECT expression for the WithKey CTE.</returns>
        protected static string BuildAssetKeyJoin(string parentAlias = "fa")
        {
            return $"INNER JOIN dbo.MaintainableAssetLocation mal ON mal.MaintainableAssetId = {parentAlias}.MaintainableAssetId";
        }

        /// <summary>
        /// Builds scalar subqueries for each attribute column. For per-year tables (AssetDetailValueIntId),
        /// only attributes modified by deterioration or treatment consequences are stored. Static network
        /// attributes (DISTRICT, COUNTY, etc.) are only in the initial summary (AssetSummaryDetailValueIntId).
        /// When useFallback is true, this method uses COALESCE to first try the per-year EAV table,
        /// then fall back to AssetSummaryDetailValueIntId (via AssetSummaryDetail correlated on
        /// MaintainableAssetId + RunId) so that static attributes always populate.
        /// </summary>
        protected string BuildAttributeSubqueries(
            List<SimpleColumnDef> attributeColumns,
            string valueTableName,
            string parentIdColumn,
            string parentAlias,
            List<SqlParameter> parameters,
            bool useFallback = false)
        {
            if (attributeColumns == null || !attributeColumns.Any())
                return "";

            var sb = new StringBuilder();
            foreach (var col in attributeColumns)
            {
                var attr = _unitOfWork.AttributeRepo.GetSingleByName(col.AttributeName);
                if (attr == null) continue;

                var paramName = $"@attr_{SanitizeParamName(col.AttributeName)}";
                if (!parameters.Any(p => p.ParameterName == paramName))
                    parameters.Add(new SqlParameter(paramName, attr.Id));

                string valueExpr = attr.Type == "NUMBER"
                    ? "CAST(v.NumericValue AS NVARCHAR(200))"
                    : "v.TextValue";

                if (useFallback)
                {
                    // COALESCE: first try per-year EAV table, then fall back to initial summary
                    // AssetSummaryDetailValueIntId stores ALL attributes at simulation start
                    // Join path: AssetSummaryDetail (RunId + MaintainableAssetId) → AssetSummaryDetailValueIntId
                    string fallbackValueExpr = attr.Type == "NUMBER"
                        ? "CAST(sv.NumericValue AS NVARCHAR(200))"
                        : "sv.TextValue";

                    sb.Append($@",
    COALESCE(
        (SELECT {valueExpr} FROM dbo.{valueTableName} v
         WHERE v.RunId = @RunId AND v.{parentIdColumn} = {parentAlias}.Id
         AND v.AttributeId = {paramName}),
        (SELECT TOP 1 {fallbackValueExpr} FROM dbo.AssetSummaryDetailValueIntId sv
         INNER JOIN dbo.AssetSummaryDetail asd2 ON asd2.RunId = sv.RunId AND asd2.Id = sv.AssetSummaryDetailId
         WHERE sv.RunId = @RunId AND asd2.MaintainableAssetId = {parentAlias}.MaintainableAssetId
         AND sv.AttributeId = {paramName})
    ) AS [{col.AttributeName}]");
                }
                else
                {
                    sb.Append($@",
    (SELECT {valueExpr} FROM dbo.{valueTableName} v
     WHERE v.RunId = @RunId AND v.{parentIdColumn} = {parentAlias}.Id
     AND v.AttributeId = {paramName}) AS [{col.AttributeName}]");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds a set-based conditional-aggregation CTE pivot for attribute enrichment.
        /// Replaces the previous N-LEFT-JOIN pattern which caused nested loop seeks on the rowstore
        /// clustered index and bypassed the NCCI. This pattern lets the optimizer issue one batch-mode
        /// columnstore scan over the paged row set (typically 50 rows) and pivot in memory.
        ///
        /// When useFallback is false (GetAssetSummaryData): emits PivotedAttrs scanning
        /// AssetSummaryDetailValueIntId for the paged IDs.
        ///
        /// When useFallback is true (yearly methods): emits PivotedAttrs scanning
        /// AssetDetailValueIntId for the paged AssetDetail IDs, plus PivotedSummary scanning
        /// AssetSummaryDetailValueIntId for the corresponding MaintainableAsset IDs. The final
        /// SELECT uses COALESCE(pa.[X], ps.[X]) to resolve static attributes missing from per-year EAV.
        /// </summary>
        protected AttributePivotCteResult BuildAttributePivotCte(
            List<SimpleColumnDef> attributeColumns,
            string valueTableName,
            string parentIdColumn,
            List<SqlParameter> parameters,
            bool useFallback = false)
        {
            var result = new AttributePivotCteResult();
            if (attributeColumns == null || !attributeColumns.Any())
                return result;

            // Resolve attribute metadata and build param list
            var resolvedAttrs = new List<(SimpleColumnDef Col, string ParamName, bool IsNumber)>();
            foreach (var col in attributeColumns)
            {
                var attr = _unitOfWork.AttributeRepo.GetSingleByName(col.AttributeName);
                if (attr == null) continue;

                var paramName = $"@attr_{SanitizeParamName(col.AttributeName)}";
                if (!parameters.Any(p => p.ParameterName == paramName))
                    parameters.Add(new SqlParameter(paramName, attr.Id));

                resolvedAttrs.Add((col, paramName, attr.Type == "NUMBER"));
            }

            if (!resolvedAttrs.Any())
                return result;

            static string BuildPivotCases(
                IEnumerable<(SimpleColumnDef Col, string ParamName, bool IsNumber)> attrs,
                string alias)
            {
                var pivotCases = new StringBuilder();
                foreach (var (col, paramName, isNumber) in attrs)
                {
                    string valueCol = isNumber ? "NumericValue" : "TextValue";
                    pivotCases.AppendLine($"        MAX(CASE WHEN {alias}.AttributeId = {paramName} THEN {alias}.{valueCol} END) AS [{col.AttributeName}],");
                }

                return pivotCases.ToString().TrimEnd().TrimEnd(',');
            }

            // Build SELECT column expressions (all reference pa.[] and optionally ps.[])
            var selectCols = new StringBuilder();
            foreach (var (col, _, isNumber) in resolvedAttrs)
            {
                if (useFallback)
                {
                    if (isNumber)
                        selectCols.AppendLine($"    , CAST(COALESCE(pa.[{col.AttributeName}], ps.[{col.AttributeName}]) AS NVARCHAR(200)) AS [{col.AttributeName}]");
                    else
                        selectCols.AppendLine($"    , COALESCE(pa.[{col.AttributeName}], ps.[{col.AttributeName}]) AS [{col.AttributeName}]");
                }
                else
                {
                    if (isNumber)
                        selectCols.AppendLine($"    , CAST(pa.[{col.AttributeName}] AS NVARCHAR(200)) AS [{col.AttributeName}]");
                    else
                        selectCols.AppendLine($"    , pa.[{col.AttributeName}] AS [{col.AttributeName}]");
                }
            }

            if (useFallback)
            {
                // PivotedAttrs: one scan of per-year EAV for the paged AssetDetail IDs
                // PivotedSummary: one scan of initial summary EAV for the corresponding MaintainableAsset IDs
                // Both use IN (SELECT ... FROM PagedRows) so the optimizer can batch-scan the NCCI partition
                string pivotCasesStr = BuildPivotCases(resolvedAttrs, "adv");
                string summaryPivotCasesStr = BuildPivotCases(resolvedAttrs, "asdv");

                result.CteSql = $@"PivotedAttrs AS (
    SELECT
        adv.AssetDetailId,
{pivotCasesStr}
    FROM dbo.{valueTableName} adv
    WHERE adv.RunId = @RunId
      AND adv.AssetDetailId IN (SELECT Id FROM PagedRows)
    GROUP BY adv.AssetDetailId
),
PivotedSummary AS (
    SELECT
        asd.MaintainableAssetId,
{summaryPivotCasesStr}
    FROM dbo.AssetSummaryDetailValueIntId asdv
    INNER JOIN dbo.AssetSummaryDetail asd
        ON asd.RunId = @RunId AND asd.Id = asdv.AssetSummaryDetailId
    WHERE asdv.RunId = @RunId
      AND asd.MaintainableAssetId IN (SELECT MaintainableAssetId FROM PagedRows)
    GROUP BY asd.MaintainableAssetId
)";
                result.JoinClause = @"LEFT JOIN PivotedAttrs   pa ON pa.AssetDetailId       = pg.Id
LEFT JOIN PivotedSummary ps ON ps.MaintainableAssetId = pg.MaintainableAssetId";
            }
            else
            {
                // No fallback: one scan of the summary EAV table for the paged AssetSummaryDetail IDs
                string pivotCasesStr = BuildPivotCases(resolvedAttrs, "asdv");

                result.CteSql = $@"PivotedAttrs AS (
    SELECT
        asdv.{parentIdColumn},
{pivotCasesStr}
    FROM dbo.{valueTableName} asdv
    WHERE asdv.RunId = @RunId
      AND asdv.{parentIdColumn} IN (SELECT Id FROM PagedRows)
    GROUP BY asdv.{parentIdColumn}
)";
                result.JoinClause = "LEFT JOIN PivotedAttrs pa ON pa.AssetSummaryDetailId = pg.Id";
            }

            result.SelectColumns = selectCols.ToString().TrimEnd();
            return result;
        }

        protected CriteriaSqlFragment TranslateSchemaCriteria(
            string criteriaExpression,
            string valueTableName,
            string parentIdColumn,
            string parentAlias,
            List<SqlParameter> parameters,
            bool useSummaryFallback = false,
            string fallbackAssetDetailJoinSql = "",
            string fallbackAssetDetailWhereSql = "")
        {
            var fragment = _criteriaEmitter.EmitWhere(
                criteriaExpression, _unitOfWork.AttributeRepo,
                valueTableName, parentIdColumn, parentAlias, "sc",
                useSummaryFallback, fallbackAssetDetailJoinSql, fallbackAssetDetailWhereSql);

            AddFragmentParameters(fragment, parameters);

            return fragment;
        }

        protected CriteriaSqlFragment TranslateViewerFilters(
            Dictionary<string, string> columnFilters,
            string valueTableName,
            string parentIdColumn,
            string parentAlias,
            List<SqlParameter> parameters,
            bool useSummaryFallback = false,
            string fallbackAssetDetailJoinSql = "",
            string fallbackAssetDetailWhereSql = "",
            string assetKeyColumnName = null)
        {
            Dictionary<string, string> directColumnExpressions = null;
            if (!string.IsNullOrWhiteSpace(assetKeyColumnName))
            {
                directColumnExpressions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [assetKeyColumnName] = $@"(
                        SELECT TOP 1 malFilter.LocationIdentifier
                        FROM dbo.MaintainableAssetLocation malFilter
                        WHERE malFilter.MaintainableAssetId = {parentAlias}.MaintainableAssetId
                    )"
                };
            }

            var fragment = _criteriaEmitter.EmitColumnFilterWhere(
                columnFilters, _unitOfWork.AttributeRepo,
                valueTableName, parentIdColumn, parentAlias,
                AssetDetailSimulationFields, useSummaryFallback,
                fallbackAssetDetailJoinSql, fallbackAssetDetailWhereSql,
                directColumnExpressions);

            AddFragmentParameters(fragment, parameters);

            return fragment;
        }

        protected SortExpressionResult BuildSortExpression(
            string sortBy,
            string sortDirection,
            string valueTableName,
            string parentIdColumn,
            string parentAlias,
            List<SqlParameter> parameters,
            string defaultSort,
            bool useSummaryFallback = false,
            string assetKeyColumnName = null)
        {
            var direction = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

            if (string.IsNullOrWhiteSpace(sortBy))
                return new SortExpressionResult { SortColumnRef = $"{defaultSort} {direction}" };

            if (!string.IsNullOrWhiteSpace(assetKeyColumnName) &&
                string.Equals(sortBy, assetKeyColumnName, StringComparison.OrdinalIgnoreCase))
            {
                return new SortExpressionResult { SortColumnRef = $"{parentAlias}._AssetKey {direction}" };
            }

            // Check if it's a simulation field — direct column reference, no join needed
            if (AssetDetailSimulationFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            {
                var colName = MapSimFieldToSqlColumn(sortBy);
                return new SortExpressionResult { SortColumnRef = $"{parentAlias}.{colName} {direction}" };
            }

            // EAV attribute sort — pre-compute in WithKey via a set-based subquery join
            var (sortColRef, withKeyJoin) = _criteriaEmitter.EmitAttributeSortJoin(
                sortBy, _unitOfWork.AttributeRepo,
                valueTableName, parentIdColumn, parameters, useSummaryFallback);

            return new SortExpressionResult
            {
                SortColumnRef = $"{sortColRef} {direction}",
                WithKeyJoin = withKeyJoin
            };
        }

        protected static void AddFragmentParameters(CriteriaSqlFragment fragment, List<SqlParameter> parameters)
        {
            foreach (var p in fragment.Parameters)
            {
                if (!parameters.Any(existing => existing.ParameterName == p.ParameterName))
                    parameters.Add(p);
            }
        }

        protected static string BuildFragmentCtePrefix(params CriteriaSqlFragment[] fragments)
        {
            var ctes = fragments
                .Where(f => !string.IsNullOrWhiteSpace(f?.CriteriaCte))
                .Select(f => f.CriteriaCte)
                .ToList();

            return ctes.Any() ? $"{string.Join(",\n", ctes)},\n" : "";
        }

        protected static string BuildFragmentJoins(params CriteriaSqlFragment[] fragments)
        {
            var joins = fragments
                .Where(f => !string.IsNullOrWhiteSpace(f?.CriteriaJoin))
                .Select(f => f.CriteriaJoin)
                .ToList();

            return joins.Any() ? $"\n    {string.Join("\n    ", joins)}" : "";
        }

        protected static (string JoinSql, string WhereSql) BuildFallbackAssetDetailScope(bool selectedYearOnly)
        {
            if (!selectedYearOnly)
                return ("", "");

            return (
                "INNER JOIN dbo.SimulationYearDetail sydFallback ON sydFallback.RunId = @RunId AND sydFallback.Id = adBase.SimulationYearDetailId",
                "AND sydFallback.Year = @SelectedYear");
        }

        protected PagedViewerResultDTO ExecutePagedQuery(
            string sql,
            List<SqlParameter> parameters,
            List<ViewerColumnDTO> columns,
            ViewerDataRequestDTO request)
        {
            var context = _unitOfWork.Context;
            var rows = new List<List<object>>();
            int totalCount = 0;

            using var connection = context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 120;

            foreach (var p in parameters)
            {
                var newParam = new SqlParameter(p.ParameterName, p.Value ?? DBNull.Value);
                command.Parameters.Add(newParam);
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                // TotalCount is always in the result set
                var totalCountOrdinal = reader.GetOrdinal("TotalCount");
                totalCount = reader.GetInt32(totalCountOrdinal);

                var row = new List<object>();
                // Map columns to reader fields
                foreach (var col in columns)
                {
                    try
                    {
                        var ordinal = reader.GetOrdinal(MapColumnKeyToSqlAlias(col.Key));
                        row.Add(reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal));
                    }
                    catch
                    {
                        row.Add(null);
                    }
                }

                rows.Add(row);
            }

            return new PagedViewerResultDTO
            {
                Columns = columns,
                Rows = rows,
                TotalRowCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        protected static string MapColumnKeyToSqlAlias(string key)
        {
            // Simulation fields map to their SQL column names
            return key.ToLowerInvariant() switch
            {
                "year" => "Year",
                "appliedtreatment" => "AppliedTreatment",
                "treatmentcause" => "TreatmentCause",
                "treatmentstatus" => "TreatmentStatus",
                "cost" => "Cost",
                "budget" => "Budget",
                "budgetname" => "BudgetName",
                "goalname" => "GoalName",
                _ => key // Attribute columns (including key attribute) use the attribute name as alias
            };
        }

        protected static string MapSimFieldToSqlColumn(string field)
        {
            return field.ToLowerInvariant() switch
            {
                "year" => "Year",
                "appliedtreatment" => "AppliedTreatment",
                "treatmentcause" => "TreatmentCause",
                "treatmentstatus" => "TreatmentStatus",
                "cost" => "Cost",
                "budget" => "Budget",
                _ => field
            };
        }

        /// <summary>
        /// Builds a set-based CTE that resolves cost and budget names for the paged asset set in one pass.
        /// Replaces the previous correlated scalar subqueries (BuildCostSubquery / BuildBudgetSubquery)
        /// which executed a 3-table join chain once per row in PagedRows (50x per page).
        /// The new CTE uses IN (SELECT Id FROM PagedRows) to batch the lookup into a single query.
        /// </summary>
        protected static CostBudgetCteResult BuildCostBudgetCte(bool hasCost, bool hasBudget)
        {
            var result = new CostBudgetCteResult();
            if (!hasCost && !hasBudget)
                return result;

            result.CteSql = @"CostBudgetData AS (
    SELECT
        tcd.AssetDetailId,
        SUM(CAST(alloc.AllocatedAmount AS FLOAT)) AS TotalCost,
        STRING_AGG(alloc.BudgetName, ', ') AS BudgetNames
    FROM dbo.TreatmentConsiderationDetail tcd
    INNER JOIN dbo.FundingCalculationOutput fco
        ON fco.RunId = tcd.RunId AND fco.TreatmentConsiderationDetailId = tcd.Id
    INNER JOIN dbo.Allocation alloc
        ON alloc.RunId = fco.RunId AND alloc.FundingCalculationOutputId = fco.Id
    WHERE tcd.RunId = @RunId
      AND tcd.AssetDetailId IN (SELECT Id FROM PagedRows)
    GROUP BY tcd.AssetDetailId
)";
            result.JoinClause = "LEFT JOIN CostBudgetData cbd ON cbd.AssetDetailId = pg.Id";
            result.CostSelectCol = hasCost ? "    , ISNULL(cbd.TotalCost, 0) AS Cost" : "";
            result.BudgetSelectCol = hasBudget ? "    , cbd.BudgetNames AS Budget" : "";
            return result;
        }

        /// <summary>
        /// SQL CASE expression to convert TreatmentCause int to human-readable label.
        /// Values: 0=Undefined, 1=No Selection, 2=Selected Treatment, 3=Scheduled Treatment,
        /// 4=Committed Project, 5=Cash Flow Project
        /// </summary>
        protected static string TreatmentCauseCaseExpression(string alias)
        {
            return $@"CASE {alias}.TreatmentCause
                WHEN 0 THEN 'Undefined'
                WHEN 1 THEN 'No Selection'
                WHEN 2 THEN 'Selected Treatment'
                WHEN 3 THEN 'Scheduled Treatment'
                WHEN 4 THEN 'Committed Project'
                WHEN 5 THEN 'Cash Flow Project'
                ELSE CAST({alias}.TreatmentCause AS NVARCHAR(50))
            END";
        }

        /// <summary>
        /// SQL CASE expression to convert TreatmentStatus int to human-readable label.
        /// Values: 0=Undefined, 1=Applied, 2=Progressed
        /// </summary>
        protected static string TreatmentStatusCaseExpression(string alias)
        {
            return $@"CASE {alias}.TreatmentStatus
                WHEN 0 THEN 'Undefined'
                WHEN 1 THEN 'Applied'
                WHEN 2 THEN 'Progressed'
                ELSE CAST({alias}.TreatmentStatus AS NVARCHAR(50))
            END";
        }

        /// <summary>
        /// Applies in-memory filtering, sorting, and pagination to a fully-materialized row list.
        /// Used by non-SQL aggregation methods (BudgetSummary, WorkSummary)
        /// that build all rows in C# and need to filter/sort/page the result before returning.
        /// </summary>
        protected static PagedViewerResultDTO ApplyInMemoryFilterSortAndPaginate(
            List<ViewerColumnDTO> columns,
            List<List<object>> allRows,
            ViewerDataRequestDTO request)
        {
            IEnumerable<List<object>> result = allRows;

            // Apply column filters (contains match on string representation)
            if (request.ColumnFilters != null)
            {
                foreach (var kvp in request.ColumnFilters)
                {
                    if (string.IsNullOrWhiteSpace(kvp.Value)) continue;
                    int colIdx = columns.FindIndex(c => c.Key == kvp.Key);
                    if (colIdx < 0 || colIdx >= columns.Count) continue;
                    if (!columns[colIdx].Filterable) continue;
                    var filterVal = kvp.Value.Trim().ToLowerInvariant();
                    result = result.Where(row =>
                        colIdx < row.Count &&
                        row[colIdx] != null &&
                        row[colIdx].ToString()!.ToLowerInvariant().Contains(filterVal));
                }
            }

            // Apply sort
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                int sortIdx = columns.FindIndex(c => c.Key == request.SortBy);
                if (sortIdx >= 0 && sortIdx < columns.Count && columns[sortIdx].Sortable)
                {
                    bool desc = request.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
                    result = desc
                        ? result.OrderByDescending(row => sortIdx < row.Count ? row[sortIdx] : null, new ObjectComparer())
                        : result.OrderBy(row => sortIdx < row.Count ? row[sortIdx] : null, new ObjectComparer());
                }
            }

            var filtered = result.ToList();
            int totalCount = filtered.Count;
            int offset = (request.Page - 1) * request.PageSize;
            var pagedRows = filtered.Skip(offset).Take(request.PageSize).ToList();

            return new PagedViewerResultDTO
            {
                Columns = columns,
                Rows = pagedRows,
                TotalRowCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        /// <summary>
        /// Generic comparer for in-memory sorting that handles mixed types (string, int, double, etc.)
        /// by comparing numeric values numerically and everything else as strings.
        /// </summary>
        protected class ObjectComparer : IComparer<object?>
        {
            public int Compare(object? x, object? y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                // Try numeric comparison first
                if (x is IConvertible && y is IConvertible)
                {
                    try
                    {
                        double dx = Convert.ToDouble(x);
                        double dy = Convert.ToDouble(y);
                        return dx.CompareTo(dy);
                    }
                    catch { /* fall through to string comparison */ }
                }

                return string.Compare(x.ToString(), y.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        protected static PagedViewerResultDTO EmptyResult(ViewerDataRequestDTO request)
        {
            return new PagedViewerResultDTO
            {
                Columns = new List<ViewerColumnDTO>(),
                Rows = new List<List<object>>(),
                TotalRowCount = 0,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        protected static string SanitizeParamName(string name)
        {
            return System.Text.RegularExpressions.Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");
        }

        #endregion

        #region Config Helpers

        protected static string GetConfigString(Dictionary<string, object> config, string key, string defaultValue = "")
        {
            if (config.TryGetValue(key, out var val) && val != null)
                return val.ToString();
            return defaultValue;
        }

        protected static int? GetConfigInt(Dictionary<string, object> config, string key)
        {
            if (config.TryGetValue(key, out var val) && val != null)
            {
                if (val is int i) return i;
                if (val is long l) return (int)l;
                if (int.TryParse(val.ToString(), out var parsed)) return parsed;
            }
            return null;
        }

        protected static bool GetConfigBool(Dictionary<string, object> config, string key, bool defaultValue = false)
        {
            if (config.TryGetValue(key, out var val) && val != null)
            {
                if (val is bool b) return b;
                if (bool.TryParse(val.ToString(), out var parsed)) return parsed;
            }
            return defaultValue;
        }

        protected static List<SimpleColumnDef> GetConfigColumnDefs(Dictionary<string, object> config, string key)
        {
            if (!config.TryGetValue(key, out var val) || val == null)
                return new List<SimpleColumnDef>();

            if (val is JArray jArray)
            {
                return jArray.Select(j => new SimpleColumnDef
                {
                    AttributeName = j["attributeName"]?.ToString() ?? "",
                    DisplayHeader = j["displayHeader"]?.ToString() ?? j["attributeName"]?.ToString() ?? "",
                    Format = j["format"]?.ToString() ?? "text"
                }).ToList();
            }

            return new List<SimpleColumnDef>();
        }

        protected static List<SimpleSimulationColumnDef> GetConfigSimulationColumnDefs(Dictionary<string, object> config, string key)
        {
            if (!config.TryGetValue(key, out var val) || val == null)
                return new List<SimpleSimulationColumnDef>();

            if (val is JArray jArray)
            {
                return jArray.Select(j => new SimpleSimulationColumnDef
                {
                    Field = j["field"]?.ToString() ?? "",
                    DisplayHeader = j["displayHeader"]?.ToString() ?? j["field"]?.ToString() ?? "",
                    Format = j["format"]?.ToString() ?? "text"
                }).ToList();
            }

            return new List<SimpleSimulationColumnDef>();
        }

        /// <summary>
        /// Reads a config value that is a string array (e.g., conditionAttributes: ["IRI", "OPI"]).
        /// </summary>
        protected static List<string> GetConfigStringArray(Dictionary<string, object> config, string key)
        {
            if (!config.TryGetValue(key, out var val) || val == null)
                return new List<string>();

            if (val is JArray jArray)
                return jArray.Select(j => j.ToString()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            return new List<string>();
        }

        #endregion

        #region Internal Types

        protected internal class SimpleColumnDef
        {
            public string AttributeName { get; set; }
            public string DisplayHeader { get; set; }
            public string Format { get; set; }
        }

        protected internal class SimpleSimulationColumnDef
        {
            public string Field { get; set; }
            public string DisplayHeader { get; set; }
            public string Format { get; set; }
        }

        protected internal class KeyAttributeInfo
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
        }

        protected internal class AttributePivotCteResult
        {
            public string CteSql { get; set; } = "";
            public string JoinClause { get; set; } = "";
            public string SelectColumns { get; set; } = "";
        }

        protected internal class CostBudgetCteResult
        {
            public string CteSql { get; set; } = "";
            public string JoinClause { get; set; } = "";
            public string CostSelectCol { get; set; } = "";
            public string BudgetSelectCol { get; set; } = "";
        }

        protected internal class SortExpressionResult
        {
            public string SortColumnRef { get; set; } = "";
            public string WithKeyJoin { get; set; } = "";
        }
        #endregion

        protected static ComponentQuery CreateQuery(string sql, List<SqlParameter> parameters, List<ViewerColumnDTO> columns, bool requiresInMemoryPagination = false)
        {
            return new ComponentQuery(sql, parameters, columns.Select(ToViewerColumn).ToList(), requiresInMemoryPagination);
        }

        protected static ComponentQuery CreateInMemoryQuery(List<ViewerColumnDTO> columns, List<List<object>> rows)
        {
            return new ComponentQuery(
                string.Empty,
                Array.Empty<SqlParameter>(),
                columns.Select(ToViewerColumn).ToList(),
                RequiresInMemoryPagination: true,
                cancellationToken => YieldRowsAsync(rows, cancellationToken));
        }

        private static async IAsyncEnumerable<List<object>> YieldRowsAsync(
            IEnumerable<List<object>> rows,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return row;
            }

            await Task.CompletedTask;
        }

        protected static List<List<object>> ApplyInMemoryFilterAndSort(
            List<ViewerColumnDTO> columns,
            IEnumerable<List<object>> rows,
            ViewerDataRequest request)
        {
            IEnumerable<List<object>> result = rows;

            if (request.ColumnFilters != null)
            {
                foreach (var kvp in request.ColumnFilters)
                {
                    if (string.IsNullOrWhiteSpace(kvp.Value))
                    {
                        continue;
                    }

                    var colIdx = columns.FindIndex(c => c.Key == kvp.Key);
                    if (colIdx < 0 || colIdx >= columns.Count || !columns[colIdx].Filterable)
                    {
                        continue;
                    }

                    var filterVal = kvp.Value.Trim().ToLowerInvariant();
                    result = result.Where(row =>
                        colIdx < row.Count &&
                        row[colIdx] != null &&
                        row[colIdx].ToString()!.ToLowerInvariant().Contains(filterVal));
                }
            }

            if (!string.IsNullOrEmpty(request.SortBy))
            {
                var sortIdx = columns.FindIndex(c => c.Key == request.SortBy);
                if (sortIdx >= 0 && sortIdx < columns.Count && columns[sortIdx].Sortable)
                {
                    var desc = request.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
                    result = desc
                        ? result.OrderByDescending(row => sortIdx < row.Count ? row[sortIdx] : null, new ObjectComparer())
                        : result.OrderBy(row => sortIdx < row.Count ? row[sortIdx] : null, new ObjectComparer());
                }
            }

            return result.ToList();
        }

        protected static ViewerColumn ToViewerColumn(ViewerColumnDTO column)
        {
            return new ViewerColumn
            {
                Key = column.Key,
                Header = column.Header,
                Format = column.Format,
                Sortable = column.Sortable,
                Filterable = column.Filterable
            };
        }
    }
}
