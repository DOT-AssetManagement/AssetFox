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
    public class FundedTreatmentListDataSource : ComponentDataSourceBase, IComponentDataSource
    {
        public FundedTreatmentListDataSource(UnitOfDataPersistenceWork unitOfWork, CriteriaSqlEmitter criteriaEmitter)
            : base(unitOfWork, criteriaEmitter)
        {
        }

        public string ComponentType => "fundedTreatmentList";

        public ComponentQuery Build(
            ReportExecutionContext executionContext,
            ViewerDataRequest request,
            int runId,
            AttributeLookup attributes) => GetFundedTreatmentListData(executionContext, request, runId);

        #region Funded Treatment List

        private ComponentQuery GetFundedTreatmentListData(ReportExecutionContext executionContext, ViewerDataRequest request, int runId)
        {
            var component = executionContext.Component;
            var config = component.Config ?? new Dictionary<string, object>();
            var attributeColumns = GetConfigColumnDefs(config, "columns");
            var simulationColumns = GetConfigSimulationColumnDefs(config, "simulationColumns");

            var keyAttrInfo = ResolveKeyAttribute(request.SimulationId);
            var keyColName = keyAttrInfo.Name ?? "Asset ID";

            var selectedFields = new HashSet<string>(
                simulationColumns.Select(sc => sc.Field), StringComparer.OrdinalIgnoreCase);
            bool hasCost = selectedFields.Contains("cost");
            bool hasBudget = selectedFields.Contains("budget");

            var columns = new List<ViewerColumnDTO>();
            columns.Add(new ViewerColumnDTO { Key = keyColName, Header = keyColName, Format = "text" });
            columns.Add(new ViewerColumnDTO { Key = "year", Header = "Year", Format = "year" });
            columns.Add(new ViewerColumnDTO { Key = "appliedTreatment", Header = "Treatment", Format = "text" });

            foreach (var simCol in simulationColumns.Where(sc =>
                HardcodedFundedTreatmentFields.Contains(sc.Field, StringComparer.OrdinalIgnoreCase)
                && !sc.Field.Equals("year", StringComparison.OrdinalIgnoreCase)
                && !sc.Field.Equals("appliedTreatment", StringComparison.OrdinalIgnoreCase)
                && !sc.Field.Equals("cost", StringComparison.OrdinalIgnoreCase)
                && !sc.Field.Equals("budget", StringComparison.OrdinalIgnoreCase)))
            {
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

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@RunId", runId),
                new SqlParameter("@Offset", (request.Page - 1) * request.PageSize),
                new SqlParameter("@PageSize", request.PageSize)
            };

            var calculatedFields = BuildCalculatedFieldProjection(
                executionContext,
                attributeColumns,
                ReportCalculatedFieldRowScope.Yearly,
                parameters);
            AddConfiguredViewerColumns(columns, attributeColumns, calculatedFields);
            var nativeAttributeColumns = NativeAttributeColumns(attributeColumns);

            var assetKeyJoin = BuildAssetKeyJoin("fa");
            var attrPivot = BuildAttributePivotCte(nativeAttributeColumns, "AssetDetailValueIntId", "AssetDetailId", parameters, useFallback: true);
            var costBudget = BuildCostBudgetCte(hasCost, hasBudget);
            var criteriaFragment = TranslateSchemaCriteria(component.CriteriaExpression, "AssetDetailValueIntId", "AssetDetailId", "ad", parameters, useSummaryFallback: true);
            var nativeFilters = NativeViewerFilters(request.ColumnFilters, calculatedFields);
            var filterFragment = TranslateViewerFilters(nativeFilters, "AssetDetailValueIntId", "AssetDetailId", "ad", parameters,
                useSummaryFallback: true, assetKeyColumnName: keyColName);
            var calculatedFilterWhere = BuildCalculatedFieldFilterWhere(request.ColumnFilters, calculatedFields, parameters);
            var sortResult = TryBuildCalculatedFieldSort(request.SortBy, request.SortDirection, calculatedFields, out var calculatedSort)
                ? new SortExpressionResult { SortColumnRef = calculatedSort }
                : BuildSortExpression(request.SortBy, request.SortDirection, "AssetDetailValueIntId", "AssetDetailId", "wk", parameters, $"wk._AssetKey, wk.Year",
                    useSummaryFallback: true, assetKeyColumnName: keyColName);

            var fragmentCtePrefix = BuildFragmentCtePrefix(criteriaFragment, filterFragment);
            var fragmentJoins = BuildFragmentJoins(criteriaFragment, filterFragment);
            var criteriaWhere = criteriaFragment.IsEmpty ? "" : $"AND {criteriaFragment.WhereClause}";
            var filterWhere = filterFragment.IsEmpty ? "" : $"AND {filterFragment.WhereClause}";
            var calculatedWhere = string.IsNullOrWhiteSpace(calculatedFilterWhere) ? "" : $"AND {calculatedFilterWhere}";
            var calculatedCtePrefix = calculatedFields.IsEmpty ? "" : $"{calculatedFields.CteSql},\n";
            var calculatedFilteredJoin = calculatedFields.IsEmpty ? "" : $"\n    {calculatedFields.Join("ad")}";
            var calculatedWithKeyJoin = calculatedFields.IsEmpty ? "" : $"\n    {calculatedFields.Join("fa")}";

            var pivotCteBlock = string.IsNullOrEmpty(attrPivot.CteSql) ? "" : $",\n{attrPivot.CteSql}";
            var costCteBlock = string.IsNullOrEmpty(costBudget.CteSql) ? "" : $",\n{costBudget.CteSql}";
            var withKeySortJoin = string.IsNullOrEmpty(sortResult.WithKeyJoin) ? "" : $"\n    {sortResult.WithKeyJoin}";

            var sql = $@"
;WITH {calculatedCtePrefix}{fragmentCtePrefix}FilteredAssets AS (
    SELECT ad.Id, ad.MaintainableAssetId, syd.Year, ad.AppliedTreatment,
        ({TreatmentCauseCaseExpression("ad")}) AS TreatmentCause,
        ({TreatmentStatusCaseExpression("ad")}) AS TreatmentStatus
    FROM dbo.AssetDetail ad
    INNER JOIN dbo.SimulationYearDetail syd ON syd.RunId = @RunId AND syd.Id = ad.SimulationYearDetailId{fragmentJoins}{calculatedFilteredJoin}
    WHERE ad.RunId = @RunId
    AND ad.AppliedTreatment IS NOT NULL AND ad.AppliedTreatment <> 'No Treatment'
    {criteriaWhere}
    {filterWhere}
    {calculatedWhere}
),
Counted AS (
    SELECT COUNT(*) AS TotalCount FROM FilteredAssets
),
WithKey AS (
    SELECT fa.Id, fa.MaintainableAssetId, fa.Year, fa.AppliedTreatment, fa.TreatmentCause, fa.TreatmentStatus{calculatedFields.SelectForCarry()},
        mal.LocationIdentifier AS _AssetKey
    FROM FilteredAssets fa
    {assetKeyJoin}{calculatedWithKeyJoin}{withKeySortJoin}
),
SortedAssets AS (
    SELECT wk.*, ROW_NUMBER() OVER (ORDER BY {sortResult.SortColumnRef}) AS RowNum
    FROM WithKey wk
),
PagedRows AS (
    SELECT pg.Id, pg.MaintainableAssetId, pg._AssetKey, pg.Year, pg.AppliedTreatment,
        pg.TreatmentCause, pg.TreatmentStatus, pg.RowNum{calculatedFields.Carry("pg")}
    FROM SortedAssets pg
    WHERE pg.RowNum BETWEEN @Offset + 1 AND @Offset + @PageSize
){pivotCteBlock}{costCteBlock}
SELECT pg._AssetKey AS [{keyColName}], pg.Year, pg.AppliedTreatment, pg.TreatmentCause, pg.TreatmentStatus,
    c.TotalCount
{costBudget.CostSelectCol}
{costBudget.BudgetSelectCol}
{attrPivot.SelectColumns}
{calculatedFields.FinalSelect("pg")}
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
