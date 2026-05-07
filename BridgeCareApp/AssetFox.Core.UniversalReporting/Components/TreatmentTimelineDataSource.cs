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
    public class TreatmentTimelineDataSource : ComponentDataSourceBase, IComponentDataSource
    {
        public TreatmentTimelineDataSource(UnitOfDataPersistenceWork unitOfWork, CriteriaSqlEmitter criteriaEmitter)
            : base(unitOfWork, criteriaEmitter)
        {
        }

        public string ComponentType => "treatmentTimeline";

        public ComponentQuery Build(
            ReportComponent component,
            ViewerDataRequest request,
            int runId,
            AttributeLookup attributes) => GetTreatmentTimelineData(component, request, runId);

        #region Treatment Timeline

        private ComponentQuery GetTreatmentTimelineData(ReportComponent component, ViewerDataRequest request, int runId)
        {
            var config = component.Config ?? new Dictionary<string, object>();
            var attributeColumns = GetConfigColumnDefs(config, "attributeColumns");
            var showNoTreatment = GetConfigBool(config, "showNoTreatment");

            var allAttrColumns = new List<SimpleColumnDef>(attributeColumns);

            var keyAttrInfo = ResolveKeyAttribute(request.SimulationId);
            var keyColName = keyAttrInfo.Name ?? "Asset ID";

            var columns = new List<ViewerColumnDTO>();
            columns.Add(new ViewerColumnDTO { Key = keyColName, Header = keyColName, Format = "text" });
            columns.Add(new ViewerColumnDTO { Key = "year", Header = "Year", Format = "year", Filterable = true });
            columns.Add(new ViewerColumnDTO { Key = "appliedTreatment", Header = "Treatment", Format = "text" });
            columns.Add(new ViewerColumnDTO { Key = "treatmentCause", Header = "Treatment Cause", Format = "text" });

            foreach (var attrCol in allAttrColumns)
                columns.Add(new ViewerColumnDTO { Key = attrCol.AttributeName, Header = attrCol.DisplayHeader, Format = attrCol.Format });

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@RunId", runId),
                new SqlParameter("@Offset", (request.Page - 1) * request.PageSize),
                new SqlParameter("@PageSize", request.PageSize)
            };

            var noTreatmentWhere = "";
            if (!showNoTreatment)
            {
                parameters.Add(new SqlParameter("@NoTreatmentName", "No Treatment"));
                noTreatmentWhere = "AND (ad.AppliedTreatment IS NULL OR ad.AppliedTreatment <> @NoTreatmentName)";
            }

            var assetKeyJoin = BuildAssetKeyJoin("fa");
            var attrPivot = BuildAttributePivotCte(allAttrColumns, "AssetDetailValueIntId", "AssetDetailId", parameters, useFallback: true);
            var criteriaFragment = TranslateSchemaCriteria(component.CriteriaExpression, "AssetDetailValueIntId", "AssetDetailId", "ad", parameters, useSummaryFallback: true);
            var filterFragment = TranslateViewerFilters(request.ColumnFilters, "AssetDetailValueIntId", "AssetDetailId", "ad", parameters,
                useSummaryFallback: true, assetKeyColumnName: keyColName);
            var sortResult = BuildSortExpression(request.SortBy, request.SortDirection, "AssetDetailValueIntId", "AssetDetailId", "wk", parameters, $"wk._AssetKey, wk.Year",
                useSummaryFallback: true, assetKeyColumnName: keyColName);

            var fragmentCtePrefix = BuildFragmentCtePrefix(criteriaFragment, filterFragment);
            var fragmentJoins = BuildFragmentJoins(criteriaFragment, filterFragment);
            var criteriaWhere = criteriaFragment.IsEmpty ? "" : $"AND {criteriaFragment.WhereClause}";
            var filterWhere = filterFragment.IsEmpty ? "" : $"AND {filterFragment.WhereClause}";

            var pivotCteBlock = string.IsNullOrEmpty(attrPivot.CteSql) ? "" : $",\n{attrPivot.CteSql}";
            var withKeySortJoin = string.IsNullOrEmpty(sortResult.WithKeyJoin) ? "" : $"\n    {sortResult.WithKeyJoin}";

            var sql = $@"
;WITH {fragmentCtePrefix}FilteredAssets AS (
    SELECT ad.Id, ad.MaintainableAssetId, syd.Year, ad.AppliedTreatment,
        ({TreatmentCauseCaseExpression("ad")}) AS TreatmentCause,
        ({TreatmentStatusCaseExpression("ad")}) AS TreatmentStatus
    FROM dbo.AssetDetail ad
    INNER JOIN dbo.SimulationYearDetail syd ON syd.RunId = @RunId AND syd.Id = ad.SimulationYearDetailId{fragmentJoins}
    WHERE ad.RunId = @RunId
    {noTreatmentWhere}
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
){pivotCteBlock}
SELECT pg._AssetKey AS [{keyColName}], pg.Year, pg.AppliedTreatment, pg.TreatmentCause, pg.TreatmentStatus,
    c.TotalCount
{attrPivot.SelectColumns}
FROM PagedRows pg
CROSS JOIN Counted c
{attrPivot.JoinClause}
ORDER BY pg.RowNum";

            return CreateQuery(sql, parameters, columns);
        }

        #endregion


    }
}