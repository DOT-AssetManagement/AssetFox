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

namespace AssetFox.Core.UniversalReporting.Components
{
    public class AssetDetailDataSource : ComponentDataSourceBase, IComponentDataSource
    {
        public AssetDetailDataSource(UnitOfDataPersistenceWork unitOfWork, CriteriaSqlEmitter criteriaEmitter)
            : base(unitOfWork, criteriaEmitter)
        {
        }

        public string ComponentType => "assetDetail";

        public ComponentQuery Build(
            ReportComponent component,
            ViewerDataRequest request,
            int runId,
            AttributeLookup attributes) => GetAssetDetailData(component, request, runId);

        #region Asset Detail

        private ComponentQuery GetAssetDetailData(ReportComponent component, ViewerDataRequest request, int runId)
        {
            var config = component.Config ?? new Dictionary<string, object>();
            var timeScope = GetConfigString(config, "timeScope", "allYears");

            if (timeScope == "initial")
                return GetAssetSummaryData(component, request, runId);

            return GetAssetYearlyData(component, request, runId, timeScope);
        }

        private ComponentQuery GetAssetSummaryData(ReportComponent component, ViewerDataRequest request, int runId)
        {
            var config = component.Config ?? new Dictionary<string, object>();
            var attributeColumns = GetConfigColumnDefs(config, "attributeColumns");

            var keyAttrInfo = ResolveKeyAttribute(request.SimulationId);
            var keyColName = keyAttrInfo.Name ?? "Asset ID";

            var columns = new List<ViewerColumnDTO>();
            columns.Add(new ViewerColumnDTO { Key = keyColName, Header = keyColName, Format = "text" });

            foreach (var attrCol in attributeColumns)
                columns.Add(new ViewerColumnDTO { Key = attrCol.AttributeName, Header = attrCol.DisplayHeader, Format = attrCol.Format });

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@RunId", runId),
                new SqlParameter("@Offset", (request.Page - 1) * request.PageSize),
                new SqlParameter("@PageSize", request.PageSize)
            };

            var assetKeyJoin = BuildAssetKeyJoin("fa");

            var attrPivot = BuildAttributePivotCte(attributeColumns, "AssetSummaryDetailValueIntId", "AssetSummaryDetailId", parameters, useFallback: false);

            var criteriaFragment = TranslateSchemaCriteria(component.CriteriaExpression, "AssetSummaryDetailValueIntId", "AssetSummaryDetailId", "asd", parameters);
            var filterFragment = TranslateViewerFilters(request.ColumnFilters, "AssetSummaryDetailValueIntId", "AssetSummaryDetailId", "asd", parameters, assetKeyColumnName: keyColName);
            var sortResult = BuildSortExpression(request.SortBy, request.SortDirection, "AssetSummaryDetailValueIntId", "AssetSummaryDetailId", "wk", parameters, $"wk._AssetKey", assetKeyColumnName: keyColName);

            var fragmentCtePrefix = BuildFragmentCtePrefix(criteriaFragment, filterFragment);
            var fragmentJoins = BuildFragmentJoins(criteriaFragment, filterFragment);
            var criteriaWhere = criteriaFragment.IsEmpty ? "" : $"AND {criteriaFragment.WhereClause}";
            var filterWhere = filterFragment.IsEmpty ? "" : $"AND {filterFragment.WhereClause}";

            var pivotCteBlock = string.IsNullOrEmpty(attrPivot.CteSql) ? "" : $",\n{attrPivot.CteSql}";
            var withKeySortJoin = string.IsNullOrEmpty(sortResult.WithKeyJoin) ? "" : $"\n    {sortResult.WithKeyJoin}";

            var sql = $@"
;WITH {fragmentCtePrefix}FilteredAssets AS (
    SELECT asd.Id, asd.MaintainableAssetId
    FROM dbo.AssetSummaryDetail asd{fragmentJoins}
    WHERE asd.RunId = @RunId
    {criteriaWhere}
    {filterWhere}
),
Counted AS (
    SELECT COUNT(*) AS TotalCount FROM FilteredAssets
),
WithKey AS (
    SELECT fa.Id, fa.MaintainableAssetId,
        mal.LocationIdentifier AS _AssetKey
    FROM FilteredAssets fa
    {assetKeyJoin}{withKeySortJoin}
),
SortedAssets AS (
    SELECT wk.*, ROW_NUMBER() OVER (ORDER BY {sortResult.SortColumnRef}) AS RowNum
    FROM WithKey wk
),
PagedRows AS (
    SELECT pg.Id, pg.MaintainableAssetId, pg._AssetKey, pg.RowNum
    FROM SortedAssets pg
    WHERE pg.RowNum BETWEEN @Offset + 1 AND @Offset + @PageSize
){pivotCteBlock}
SELECT pg._AssetKey AS [{keyColName}], c.TotalCount
{attrPivot.SelectColumns}
FROM PagedRows pg
CROSS JOIN Counted c
{attrPivot.JoinClause}
ORDER BY pg.RowNum";

            return CreateQuery(sql, parameters, columns);
        }

        private ComponentQuery GetAssetYearlyData(ReportComponent component, ViewerDataRequest request, int runId, string timeScope)
        {
            var config = component.Config ?? new Dictionary<string, object>();
            var attributeColumns = GetConfigColumnDefs(config, "attributeColumns");
            var simulationColumns = GetConfigSimulationColumnDefs(config, "simulationColumns");
            int? selectedYear = GetConfigInt(config, "selectedYear");

            var keyAttrInfo = ResolveKeyAttribute(request.SimulationId);
            var keyColName = keyAttrInfo.Name ?? "Asset ID";

            var selectedFields = new HashSet<string>(
                simulationColumns.Select(sc => sc.Field), StringComparer.OrdinalIgnoreCase);
            bool hasCost = selectedFields.Contains("cost");
            bool hasBudget = selectedFields.Contains("budget");

            var columns = new List<ViewerColumnDTO>();
            columns.Add(new ViewerColumnDTO { Key = keyColName, Header = keyColName, Format = "text" });

            if (timeScope == "allYears")
                columns.Add(new ViewerColumnDTO { Key = "year", Header = "Year", Format = "year", Filterable = true });

            foreach (var simCol in simulationColumns.Where(sc =>
                HardcodedYearlyFields.Contains(sc.Field, StringComparer.OrdinalIgnoreCase)
                && !sc.Field.Equals("cost", StringComparison.OrdinalIgnoreCase)
                && !sc.Field.Equals("budget", StringComparison.OrdinalIgnoreCase)))
            {
                if (simCol.Field.Equals("year", StringComparison.OrdinalIgnoreCase))
                    columns.Add(new ViewerColumnDTO { Key = simCol.Field, Header = simCol.DisplayHeader, Format = "year" });
                else
                    columns.Add(new ViewerColumnDTO { Key = simCol.Field, Header = simCol.DisplayHeader, Format = simCol.Format });
            }

            if (hasCost)
                columns.Add(new ViewerColumnDTO { Key = "cost", Header = simulationColumns.First(sc =>
                    sc.Field.Equals("cost", StringComparison.OrdinalIgnoreCase)).DisplayHeader, Format = "currency",
                    Sortable = false, Filterable = false });
            if (hasBudget)
                columns.Add(new ViewerColumnDTO { Key = "budget", Header = simulationColumns.First(sc =>
                    sc.Field.Equals("budget", StringComparison.OrdinalIgnoreCase)).DisplayHeader, Format = "text",
                    Sortable = false, Filterable = false });

            foreach (var attrCol in attributeColumns)
                columns.Add(new ViewerColumnDTO { Key = attrCol.AttributeName, Header = attrCol.DisplayHeader, Format = attrCol.Format });

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@RunId", runId),
                new SqlParameter("@Offset", (request.Page - 1) * request.PageSize),
                new SqlParameter("@PageSize", request.PageSize)
            };

            string yearFilter = "";
            if (timeScope == "year" && selectedYear.HasValue)
            {
                yearFilter = "AND syd.Year = @SelectedYear";
                parameters.Add(new SqlParameter("@SelectedYear", selectedYear.Value));
            }

            var assetKeyJoin = BuildAssetKeyJoin("fa");

            var attrPivot = BuildAttributePivotCte(attributeColumns, "AssetDetailValueIntId", "AssetDetailId", parameters, useFallback: true);
            var costBudget = BuildCostBudgetCte(hasCost, hasBudget);

            var fallbackScope = BuildFallbackAssetDetailScope(timeScope == "year" && selectedYear.HasValue);
            var criteriaFragment = TranslateSchemaCriteria(component.CriteriaExpression, "AssetDetailValueIntId", "AssetDetailId", "ad", parameters,
                useSummaryFallback: true,
                fallbackAssetDetailJoinSql: fallbackScope.JoinSql,
                fallbackAssetDetailWhereSql: fallbackScope.WhereSql);
            var filterFragment = TranslateViewerFilters(request.ColumnFilters, "AssetDetailValueIntId", "AssetDetailId", "ad", parameters,
                useSummaryFallback: true,
                fallbackAssetDetailJoinSql: fallbackScope.JoinSql,
                fallbackAssetDetailWhereSql: fallbackScope.WhereSql,
                assetKeyColumnName: keyColName);
            var sortResult = BuildSortExpression(request.SortBy, request.SortDirection, "AssetDetailValueIntId", "AssetDetailId", "wk", parameters, $"wk._AssetKey, wk.Year",
                useSummaryFallback: true, assetKeyColumnName: keyColName);

            var fragmentCtePrefix = BuildFragmentCtePrefix(criteriaFragment, filterFragment);
            var fragmentJoins = BuildFragmentJoins(criteriaFragment, filterFragment);
            var criteriaWhere = criteriaFragment.IsEmpty ? "" : $"AND {criteriaFragment.WhereClause}";
            var filterWhere = filterFragment.IsEmpty ? "" : $"AND {filterFragment.WhereClause}";

            var pivotCteBlock = string.IsNullOrEmpty(attrPivot.CteSql) ? "" : $",\n{attrPivot.CteSql}";
            var costCteBlock = string.IsNullOrEmpty(costBudget.CteSql) ? "" : $",\n{costBudget.CteSql}";
            var withKeySortJoin = string.IsNullOrEmpty(sortResult.WithKeyJoin) ? "" : $"\n    {sortResult.WithKeyJoin}";

            var sql = $@"
;WITH {fragmentCtePrefix}FilteredAssets AS (
    SELECT ad.Id, ad.MaintainableAssetId, syd.Year, ad.AppliedTreatment,
        ({TreatmentCauseCaseExpression("ad")}) AS TreatmentCause,
        ({TreatmentStatusCaseExpression("ad")}) AS TreatmentStatus
    FROM dbo.AssetDetail ad
    INNER JOIN dbo.SimulationYearDetail syd ON syd.RunId = @RunId AND syd.Id = ad.SimulationYearDetailId{fragmentJoins}
    WHERE ad.RunId = @RunId
    {yearFilter}
    {criteriaWhere}
    {filterWhere}
),
Counted AS (
    SELECT COUNT(*) AS TotalCount FROM FilteredAssets
),
WithKey AS (
    SELECT fa.Id, fa.MaintainableAssetId, fa.Year, fa.AppliedTreatment, fa.TreatmentCause, fa.TreatmentStatus,
        mal.LocationIdentifier AS _AssetKey
    FROM FilteredAssets fa
    {assetKeyJoin}{withKeySortJoin}
),
SortedAssets AS (
    SELECT wk.*, ROW_NUMBER() OVER (ORDER BY {sortResult.SortColumnRef}) AS RowNum
    FROM WithKey wk
),
PagedRows AS (
    SELECT pg.Id, pg.MaintainableAssetId, pg._AssetKey, pg.Year, pg.AppliedTreatment,
        pg.TreatmentCause, pg.TreatmentStatus, pg.RowNum
    FROM SortedAssets pg
    WHERE pg.RowNum BETWEEN @Offset + 1 AND @Offset + @PageSize
){pivotCteBlock}{costCteBlock}
SELECT pg._AssetKey AS [{keyColName}], pg.Year, pg.AppliedTreatment, pg.TreatmentCause, pg.TreatmentStatus,
    c.TotalCount
{costBudget.CostSelectCol}
{costBudget.BudgetSelectCol}
{attrPivot.SelectColumns}
FROM PagedRows pg
CROSS JOIN Counted c
{attrPivot.JoinClause}
{costBudget.JoinClause}
ORDER BY pg.RowNum";

            return CreateQuery(sql, parameters, columns);
        }

        #endregion


    }
}