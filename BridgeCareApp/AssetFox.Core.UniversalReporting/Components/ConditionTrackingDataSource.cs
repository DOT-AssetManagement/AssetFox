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
    public class ConditionTrackingDataSource : ComponentDataSourceBase, IComponentDataSource
    {
        public ConditionTrackingDataSource(UnitOfDataPersistenceWork unitOfWork, CriteriaSqlEmitter criteriaEmitter)
            : base(unitOfWork, criteriaEmitter)
        {
        }

        public string ComponentType => "conditionTracking";

        public ComponentQuery Build(
            ReportExecutionContext executionContext,
            ViewerDataRequest request,
            int runId,
            AttributeLookup attributes) => GetConditionTrackingData(executionContext, request, runId);

        #region Condition Tracking

        private ComponentQuery GetConditionTrackingData(ReportExecutionContext executionContext, ViewerDataRequest request, int runId)
        {
            var component = executionContext.Component;
            var config = component.Config ?? new Dictionary<string, object>();
            var conditionAttrNames = GetConfigStringArray(config, "conditionAttributes");
            var attributeColumns = GetConfigColumnDefs(config, "attributeColumns");
            var conditionFields = GetConfigColumnDefs(config, "conditionFields");

            var keyAttrInfo = ResolveKeyAttribute(request.SimulationId);
            var keyColName = keyAttrInfo.Name ?? "Asset ID";

            var conditionColumns = conditionFields.Count > 0
                ? conditionFields
                : conditionAttrNames.Select(name => new SimpleColumnDef
                {
                    AttributeName = name,
                    DisplayHeader = name,
                    Format = "number"
                }).ToList();

            var allAttrColumns = new List<SimpleColumnDef>();
            allAttrColumns.AddRange(attributeColumns);
            allAttrColumns.AddRange(conditionColumns);

            var columns = new List<ViewerColumnDTO>();
            columns.Add(new ViewerColumnDTO { Key = keyColName, Header = keyColName, Format = "text" });
            columns.Add(new ViewerColumnDTO { Key = "year", Header = "Year", Format = "year", Filterable = true });

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@RunId", runId),
                new SqlParameter("@Offset", (request.Page - 1) * request.PageSize),
                new SqlParameter("@PageSize", request.PageSize)
            };

            var calculatedFields = BuildCalculatedFieldProjection(
                executionContext,
                allAttrColumns,
                ReportCalculatedFieldRowScope.Yearly,
                parameters);
            AddConfiguredViewerColumns(columns, attributeColumns, calculatedFields);
            AddConfiguredViewerColumns(columns, conditionColumns, calculatedFields);
            var nativeAttributeColumns = NativeAttributeColumns(allAttrColumns);

            var assetKeyJoin = BuildAssetKeyJoin("fa");
            var attrPivot = BuildAttributePivotCte(nativeAttributeColumns, "AssetDetailValueIntId", "AssetDetailId", parameters, useFallback: true);
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
            var withKeySortJoin = string.IsNullOrEmpty(sortResult.WithKeyJoin) ? "" : $"\n    {sortResult.WithKeyJoin}";

            var sql = $@"
;WITH {calculatedCtePrefix}{fragmentCtePrefix}FilteredAssets AS (
    SELECT ad.Id, ad.MaintainableAssetId, syd.Year
    FROM dbo.AssetDetail ad
    INNER JOIN dbo.SimulationYearDetail syd ON syd.RunId = @RunId AND syd.Id = ad.SimulationYearDetailId{fragmentJoins}{calculatedFilteredJoin}
    WHERE ad.RunId = @RunId
    {criteriaWhere}
    {filterWhere}
    {calculatedWhere}
),
Counted AS (
    SELECT COUNT(*) AS TotalCount FROM FilteredAssets
),
WithKey AS (
    SELECT fa.Id, fa.MaintainableAssetId, fa.Year{calculatedFields.SelectForCarry()},
        mal.LocationIdentifier AS _AssetKey
    FROM FilteredAssets fa
    {assetKeyJoin}{calculatedWithKeyJoin}{withKeySortJoin}
),
SortedAssets AS (
    SELECT wk.*, ROW_NUMBER() OVER (ORDER BY {sortResult.SortColumnRef}) AS RowNum
    FROM WithKey wk
),
PagedRows AS (
    SELECT pg.Id, pg.MaintainableAssetId, pg._AssetKey, pg.Year, pg.RowNum{calculatedFields.Carry("pg")}
    FROM SortedAssets pg
    WHERE pg.RowNum BETWEEN @Offset + 1 AND @Offset + @PageSize
){pivotCteBlock}
SELECT pg._AssetKey AS [{keyColName}], pg.Year, c.TotalCount
{attrPivot.SelectColumns}
{calculatedFields.FinalSelect("pg")}
FROM PagedRows pg
CROSS JOIN Counted c
{attrPivot.JoinClause}
ORDER BY pg.RowNum";

            return CreateQuery(sql, parameters, columns);
        }

        #endregion


    }
}
