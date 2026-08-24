using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Query;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace AssetFox.Core.UniversalReporting.Components
{
    /// <summary>
    /// Produces one row for each qualifying unfunded asset-year. A treatment option qualifies when
    /// the analysis produced an option for an AssetDetail whose TreatmentCause is NoSelection.
    /// Considered options are preferred; if none were considered, the best available option is used.
    /// Events at and after the asset's first funded treatment are excluded to preserve the behavior
    /// of the legacy "Unfunded Treatment - Time" report.
    /// </summary>
    public class UnfundedTreatmentListDataSource : ComponentDataSourceBase, IComponentDataSource
    {
        private static readonly HashSet<string> RequiredCandidateFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "year", "candidateTreatment", "estimatedCost"
        };

        private static readonly HashSet<string> OptionalCandidateFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "benefit", "conditionChange", "remainingLife", "budgetPriority"
        };

        public UnfundedTreatmentListDataSource(UnitOfDataPersistenceWork unitOfWork, CriteriaSqlEmitter criteriaEmitter)
            : base(unitOfWork, criteriaEmitter)
        {
        }

        public string ComponentType => "unfundedTreatmentList";

        public ComponentQuery Build(
            ReportExecutionContext executionContext,
            ViewerDataRequest request,
            int runId,
            AttributeLookup attributes) => GetUnfundedTreatmentListData(executionContext, request, runId);

        private ComponentQuery GetUnfundedTreatmentListData(
            ReportExecutionContext executionContext,
            ViewerDataRequest request,
            int runId)
        {
            var component = executionContext.Component;
            var config = component.Config ?? new Dictionary<string, object>();
            var attributeColumns = GetConfigColumnDefs(config, "columns");
            var candidateColumns = GetCandidateColumnDefs(config);

            var keyAttrInfo = ResolveKeyAttribute(request.SimulationId);
            var keyColName = keyAttrInfo.Name ?? "Asset ID";
            var columns = BuildViewerColumns(keyColName, candidateColumns);

            var parameters = new List<SqlParameter>
            {
                new("@RunId", runId),
                new("@Offset", (request.Page - 1) * request.PageSize),
                new("@PageSize", request.PageSize)
            };

            var calculatedFields = BuildCalculatedFieldProjection(
                executionContext,
                attributeColumns,
                ReportCalculatedFieldRowScope.Yearly,
                parameters);
            AddConfiguredViewerColumns(columns, attributeColumns, calculatedFields);
            var nativeAttributeColumns = NativeAttributeColumns(attributeColumns);

            var directFilterExpressions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [keyColName] = @"(
                    SELECT TOP 1 malFilter.LocationIdentifier
                    FROM dbo.MaintainableAssetLocation malFilter
                    WHERE malFilter.MaintainableAssetId = ad.MaintainableAssetId
                )",
                ["year"] = "syd.Year",
                ["candidateTreatment"] = "candidate.CandidateTreatment",
                ["estimatedCost"] = "candidate.EstimatedCost",
                ["benefit"] = "candidate.Benefit",
                ["conditionChange"] = "candidate.ConditionChange",
                ["remainingLife"] = "candidate.RemainingLife",
                ["budgetPriority"] = "candidate.BudgetPriority"
            };

            var criteriaFragment = TranslateSchemaCriteria(
                component.CriteriaExpression ?? "",
                "AssetDetailValueIntId",
                "AssetDetailId",
                "ad",
                parameters,
                useSummaryFallback: true);
            var nativeFilters = NativeViewerFilters(request.ColumnFilters, calculatedFields);
            var filterFragment = _criteriaEmitter.EmitColumnFilterWhere(
                nativeFilters,
                _unitOfWork.AttributeRepo,
                "AssetDetailValueIntId",
                "AssetDetailId",
                "ad",
                AssetDetailSimulationFields,
                useSummaryFallback: true,
                directColumnExpressions: directFilterExpressions);
            AddFragmentParameters(filterFragment, parameters);

            var calculatedFilterWhere = BuildCalculatedFieldFilterWhere(
                request.ColumnFilters,
                calculatedFields,
                parameters);
            var sortResult = BuildUnfundedSortExpression(
                request,
                calculatedFields,
                parameters,
                keyColName);

            var assetKeyJoin = BuildAssetKeyJoin("fa");
            var attrPivot = BuildAttributePivotCte(
                nativeAttributeColumns,
                "AssetDetailValueIntId",
                "AssetDetailId",
                parameters,
                useFallback: true);
            var fragmentCtePrefix = BuildFragmentCtePrefix(criteriaFragment, filterFragment);
            var fragmentJoins = BuildFragmentJoins(criteriaFragment, filterFragment);
            var criteriaWhere = criteriaFragment.IsEmpty ? "" : $"AND {criteriaFragment.WhereClause}";
            var filterWhere = filterFragment.IsEmpty ? "" : $"AND {filterFragment.WhereClause}";
            var calculatedWhere = string.IsNullOrWhiteSpace(calculatedFilterWhere) ? "" : $"AND {calculatedFilterWhere}";
            var calculatedCtePrefix = calculatedFields.IsEmpty ? "" : $"{calculatedFields.CteSql},\n";
            var calculatedFilteredJoin = calculatedFields.IsEmpty ? "" : $"\n    {calculatedFields.Join("ad")}";
            var calculatedWithKeyJoin = calculatedFields.IsEmpty ? "" : $"\n    {calculatedFields.Join("fa")}";
            var withKeySortJoin = string.IsNullOrEmpty(sortResult.WithKeyJoin) ? "" : $"\n    {sortResult.WithKeyJoin}";
            var pivotCteBlock = string.IsNullOrEmpty(attrPivot.CteSql) ? "" : $",\n{attrPivot.CteSql}";

            var sql = $@"
;WITH {calculatedCtePrefix}{fragmentCtePrefix}ConsideredTreatments AS (
    SELECT tcd.AssetDetailId,
        CONVERT(NVARCHAR(450), tcd.TreatmentName) AS TreatmentName,
        MIN(tcd.BudgetPriorityLevel) AS BudgetPriority
    FROM dbo.TreatmentConsiderationDetail tcd
    WHERE tcd.RunId = @RunId
    GROUP BY tcd.AssetDetailId, CONVERT(NVARCHAR(450), tcd.TreatmentName)
),
CandidateOptions AS (
    SELECT tod.Id, tod.AssetDetailId, tod.TreatmentName AS CandidateTreatment,
        tod.Cost AS EstimatedCost, tod.Benefit, tod.ConditionChange, tod.RemainingLife,
        considered.BudgetPriority,
        CASE WHEN considered.AssetDetailId IS NULL THEN 0 ELSE 1 END AS WasConsidered
    FROM dbo.TreatmentOptionDetail tod
    LEFT JOIN ConsideredTreatments considered
        ON considered.AssetDetailId = tod.AssetDetailId
       AND considered.TreatmentName = CONVERT(NVARCHAR(450), tod.TreatmentName)
    WHERE tod.RunId = @RunId
),
RankedCandidates AS (
    SELECT candidate.*,
        ROW_NUMBER() OVER (
            PARTITION BY candidate.AssetDetailId
            ORDER BY candidate.WasConsidered DESC, candidate.Benefit DESC,
                candidate.CandidateTreatment ASC, candidate.Id ASC
        ) AS CandidateRank
    FROM CandidateOptions candidate
),
SelectedCandidates AS (
    SELECT AssetDetailId, CandidateTreatment, EstimatedCost, Benefit,
        ConditionChange, RemainingLife, BudgetPriority
    FROM RankedCandidates
    WHERE CandidateRank = 1
),
FirstFundedYears AS (
    SELECT funded.MaintainableAssetId, MIN(fundedYear.Year) AS FirstFundedYear
    FROM dbo.AssetDetail funded
    INNER JOIN dbo.SimulationYearDetail fundedYear
        ON fundedYear.RunId = @RunId
       AND fundedYear.Id = funded.SimulationYearDetailId
    WHERE funded.RunId = @RunId
      AND funded.TreatmentCause <> 1
    GROUP BY funded.MaintainableAssetId
),
FilteredAssets AS (
    SELECT ad.Id, ad.MaintainableAssetId, syd.Year,
        candidate.CandidateTreatment, candidate.EstimatedCost, candidate.Benefit,
        candidate.ConditionChange, candidate.RemainingLife, candidate.BudgetPriority
    FROM dbo.AssetDetail ad
    INNER JOIN dbo.SimulationYearDetail syd
        ON syd.RunId = @RunId
       AND syd.Id = ad.SimulationYearDetailId
    INNER JOIN SelectedCandidates candidate ON candidate.AssetDetailId = ad.Id
    LEFT JOIN FirstFundedYears funded ON funded.MaintainableAssetId = ad.MaintainableAssetId{fragmentJoins}{calculatedFilteredJoin}
    WHERE ad.RunId = @RunId
      AND ad.TreatmentCause = 1
      AND (funded.FirstFundedYear IS NULL OR syd.Year < funded.FirstFundedYear)
    {criteriaWhere}
    {filterWhere}
    {calculatedWhere}
),
Counted AS (
    SELECT COUNT(*) AS TotalCount FROM FilteredAssets
),
WithKey AS (
    SELECT fa.Id, fa.MaintainableAssetId, fa.Year, fa.CandidateTreatment,
        fa.EstimatedCost, fa.Benefit, fa.ConditionChange, fa.RemainingLife,
        fa.BudgetPriority{calculatedFields.SelectForCarry()},
        mal.LocationIdentifier AS _AssetKey
    FROM FilteredAssets fa
    {assetKeyJoin}{calculatedWithKeyJoin}{withKeySortJoin}
),
SortedAssets AS (
    SELECT wk.*, ROW_NUMBER() OVER (ORDER BY {sortResult.SortColumnRef}) AS RowNum
    FROM WithKey wk
),
PagedRows AS (
    SELECT pg.Id, pg.MaintainableAssetId, pg._AssetKey, pg.Year,
        pg.CandidateTreatment, pg.EstimatedCost, pg.Benefit, pg.ConditionChange,
        pg.RemainingLife, pg.BudgetPriority, pg.RowNum{calculatedFields.Carry("pg")}
    FROM SortedAssets pg
    WHERE pg.RowNum BETWEEN @Offset + 1 AND @Offset + @PageSize
){pivotCteBlock}
SELECT pg._AssetKey AS [{keyColName}], pg.Year, pg.CandidateTreatment,
    pg.EstimatedCost, pg.Benefit, pg.ConditionChange, pg.RemainingLife,
    pg.BudgetPriority, counted.TotalCount
{attrPivot.SelectColumns}
{calculatedFields.FinalSelect("pg")}
FROM PagedRows pg
CROSS JOIN Counted counted
{attrPivot.JoinClause}
ORDER BY pg.RowNum";

            return CreateQuery(sql, parameters, columns);
        }

        private SortExpressionResult BuildUnfundedSortExpression(
            ViewerDataRequest request,
            ReportCalculatedFieldProjectionPlan calculatedFields,
            List<SqlParameter> parameters,
            string keyColName)
        {
            if (string.IsNullOrWhiteSpace(request.SortBy))
            {
                return new SortExpressionResult
                {
                    SortColumnRef = "wk._AssetKey ASC, wk.Year ASC, wk.CandidateTreatment ASC, wk.Id ASC"
                };
            }

            if (TryBuildCalculatedFieldSort(
                    request.SortBy,
                    request.SortDirection,
                    calculatedFields,
                    out var calculatedSort))
            {
                return new SortExpressionResult
                {
                    SortColumnRef = $"{calculatedSort}, wk._AssetKey ASC, wk.Year ASC, wk.CandidateTreatment ASC, wk.Id ASC"
                };
            }

            var directSortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["candidateTreatment"] = "CandidateTreatment",
                ["estimatedCost"] = "EstimatedCost",
                ["benefit"] = "Benefit",
                ["conditionChange"] = "ConditionChange",
                ["remainingLife"] = "RemainingLife",
                ["budgetPriority"] = "BudgetPriority"
            };

            if (!string.IsNullOrWhiteSpace(request.SortBy)
                && directSortColumns.TryGetValue(request.SortBy, out var directColumn))
            {
                var direction = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                    ? "DESC"
                    : "ASC";
                return new SortExpressionResult
                {
                    SortColumnRef = $"wk.{directColumn} {direction}, wk._AssetKey ASC, wk.Year ASC, wk.CandidateTreatment ASC, wk.Id ASC"
                };
            }

            var sortResult = BuildSortExpression(
                request.SortBy ?? "",
                request.SortDirection ?? "asc",
                "AssetDetailValueIntId",
                "AssetDetailId",
                "wk",
                parameters,
                "wk._AssetKey",
                useSummaryFallback: true,
                assetKeyColumnName: keyColName);
            sortResult.SortColumnRef = $"{sortResult.SortColumnRef}, wk._AssetKey ASC, wk.Year ASC, wk.CandidateTreatment ASC, wk.Id ASC";
            return sortResult;
        }

        private static List<ViewerColumnDTO> BuildViewerColumns(
            string keyColName,
            List<CandidateColumnDef> candidateColumns)
        {
            var configuredByField = candidateColumns
                .GroupBy(column => column.Field, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            CandidateColumnDef ConfiguredOrDefault(string field, string header, string format) =>
                configuredByField.TryGetValue(field, out var configured)
                    ? configured
                    : new CandidateColumnDef { Field = field, DisplayHeader = header, Format = format };

            var year = ConfiguredOrDefault("year", "Year", "number");
            var treatment = ConfiguredOrDefault("candidateTreatment", "Candidate Treatment", "text");
            var cost = ConfiguredOrDefault("estimatedCost", "Estimated Cost", "currency");

            var columns = new List<ViewerColumnDTO>
            {
                new() { Key = keyColName, Header = keyColName, Format = "text" },
                // The viewer's generic number formatter adds thousands separators. Years need the
                // dedicated format so 2026 is displayed as "2026", not "2,026".
                new() { Key = "year", Header = year.DisplayHeader, Format = "year" },
                new() { Key = "candidateTreatment", Header = treatment.DisplayHeader, Format = treatment.Format },
                new() { Key = "estimatedCost", Header = cost.DisplayHeader, Format = cost.Format }
            };

            var selectedOptionalFields = candidateColumns
                .Select(column => column.Field)
                .Where(field => OptionalCandidateFields.Contains(field))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var candidateColumn in candidateColumns.Where(column => selectedOptionalFields.Contains(column.Field)))
            {
                if (columns.Any(column => column.Key.Equals(candidateColumn.Field, StringComparison.OrdinalIgnoreCase)))
                    continue;

                columns.Add(new ViewerColumnDTO
                {
                    Key = candidateColumn.Field,
                    Header = candidateColumn.DisplayHeader,
                    Format = candidateColumn.Format
                });
            }

            return columns;
        }

        private static List<CandidateColumnDef> GetCandidateColumnDefs(Dictionary<string, object> config)
        {
            if (!config.TryGetValue("candidateColumns", out var value) || value is not JArray array)
                return new List<CandidateColumnDef>();

            return array
                .Select(token => new CandidateColumnDef
                {
                    Field = token["field"]?.ToString() ?? "",
                    DisplayHeader = token["displayHeader"]?.ToString() ?? token["field"]?.ToString() ?? "",
                    Format = token["format"]?.ToString() ?? "text"
                })
                .Where(column => RequiredCandidateFields.Contains(column.Field) || OptionalCandidateFields.Contains(column.Field))
                .ToList();
        }

        private sealed class CandidateColumnDef
        {
            public string Field { get; set; } = "";
            public string DisplayHeader { get; set; } = "";
            public string Format { get; set; } = "text";
        }
    }
}
