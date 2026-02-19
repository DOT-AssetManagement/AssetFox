using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Reporting.Models.FlexibleReport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace AssetFox.Core.Reporting.Services.FlexibleReport
{
    public class ViewerDataService : IViewerDataService
    {
        private readonly UnitOfDataPersistenceWork _unitOfWork;
        private readonly CriteriaToSqlTranslator _criteriaTranslator;

        // All known simulation fields (direct columns + computed fields)
        private static readonly List<string> AssetDetailSimulationFields = new()
        {
            "year", "appliedTreatment", "treatmentCause", "treatmentStatus",
            "cost", "budget"
        };

        // Fields already hardcoded in the GetAssetYearlyData CTE (to prevent duplicate columns)
        private static readonly HashSet<string> HardcodedYearlyFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "year", "appliedTreatment", "treatmentCause", "treatmentStatus",
            "cost", "budget"
        };

        // Fields already hardcoded in the GetFundedTreatmentListData CTE
        private static readonly HashSet<string> HardcodedFundedTreatmentFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "year", "appliedTreatment", "treatmentCause", "treatmentStatus",
            "cost", "budget"
        };

        public ViewerDataService(UnitOfDataPersistenceWork unitOfWork, CriteriaToSqlTranslator criteriaTranslator)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _criteriaTranslator = criteriaTranslator ?? throw new ArgumentNullException(nameof(criteriaTranslator));
        }

        public ViewerMetadataDTO GetMetadata(Guid simulationId)
        {
            var context = _unitOfWork.Context;

            var simOutput = context.SimulationOutput
                .AsNoTracking()
                .FirstOrDefault(so => so.SimulationId == simulationId);

            if (simOutput == null)
            {
                return new ViewerMetadataDTO
                {
                    SimulationId = simulationId,
                    HasOutput = false
                };
            }

            var runId = simOutput.RunId;

            var simulation = context.Simulation
                .AsNoTracking()
                .FirstOrDefault(s => s.Id == simulationId);

            var years = context.SimulationYearDetail
                .AsNoTracking()
                .Where(y => y.RunId == runId)
                .OrderBy(y => y.Year)
                .Select(y => y.Year)
                .ToList();

            var assetCount = context.AssetSummaryDetail
                .AsNoTracking()
                .Where(a => a.RunId == runId)
                .Count();

            // Get available attribute names from one sample asset's values
            var sampleAssetId = context.AssetSummaryDetail
                .AsNoTracking()
                .Where(a => a.RunId == runId)
                .Select(a => a.Id)
                .FirstOrDefault();

            var availableAttributes = new List<string>();
            if (sampleAssetId != Guid.Empty)
            {
                var attributeIds = context.AssetSummaryDetailValueIntId
                    .AsNoTracking()
                    .Where(v => v.RunId == runId && v.AssetSummaryDetailId == sampleAssetId)
                    .Select(v => v.AttributeId)
                    .Distinct()
                    .ToList();

                var allAttributes = _unitOfWork.AttributeRepo.GetAllAttributesAbbreviated();
                availableAttributes = allAttributes
                    .Where(a => attributeIds.Contains(a.Id))
                    .Select(a => a.Name)
                    .OrderBy(n => n)
                    .ToList();
            }

            return new ViewerMetadataDTO
            {
                SimulationId = simulationId,
                SimulationName = simulation?.Name ?? "Unknown",
                RunId = runId,
                Years = years,
                AssetCount = assetCount,
                AvailableAttributes = availableAttributes,
                HasOutput = true
            };
        }

        public PagedViewerResultDTO GetComponentData(Guid schemaId, string componentId, ViewerDataRequestDTO request)
        {
            // Load the schema and find the component
            var schemaDto = _unitOfWork.ReportSchemaRepo.Get(schemaId);
            if (schemaDto == null)
                throw new InvalidOperationException($"Schema {schemaId} not found.");

            var schema = SchemaParser.Parse(schemaDto.SchemaJson);
            var component = schema.Components.FirstOrDefault(c =>
                string.Equals(c.Id, componentId, StringComparison.OrdinalIgnoreCase));

            if (component == null)
                throw new InvalidOperationException($"Component {componentId} not found in schema.");

            // Resolve RunId
            var runId = ResolveRunId(request.SimulationId);
            if (runId == 0)
                return EmptyResult(request);

            // Dispatch by component type
            return component.Type?.ToLowerInvariant() switch
            {
                "assetdetail" => GetAssetDetailData(component, request, runId),
                "treatmenttimeline" => GetTreatmentTimelineData(component, request, runId),
                "conditiontracking" => GetConditionTrackingData(component, request, runId),
                "fundedtreatmentlist" => GetFundedTreatmentListData(component, request, runId),
                "worksummary" => GetWorkSummaryData(component, request, runId),
                "budgetsummary" => GetBudgetSummaryData(component, request, runId),
                "goaltracking" => GetGoalTrackingData(component, request, runId),
                "textblock" => GetTextBlockData(component, request),
                _ => EmptyResult(request)
            };
        }

        #region Asset Detail

        private PagedViewerResultDTO GetAssetDetailData(ReportComponent component, ViewerDataRequestDTO request, int runId)
        {
            var config = component.Config ?? new Dictionary<string, object>();
            var timeScope = GetConfigString(config, "timeScope", "allYears");

            if (timeScope == "initial")
                return GetAssetSummaryData(component, request, runId);

            return GetAssetYearlyData(component, request, runId, timeScope);
        }

        private PagedViewerResultDTO GetAssetSummaryData(ReportComponent component, ViewerDataRequestDTO request, int runId)
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
            var filterFragment = TranslateViewerFilters(request.ColumnFilters, "AssetSummaryDetailValueIntId", "AssetSummaryDetailId", "asd", parameters);
            var sortResult = BuildSortExpression(request.SortBy, request.SortDirection, "AssetSummaryDetailValueIntId", "AssetSummaryDetailId", "wk", parameters, $"wk._AssetKey");

            var criteriaWhere = criteriaFragment.IsEmpty ? "" : $"AND {criteriaFragment.WhereClause}";
            var filterWhere = filterFragment.IsEmpty ? "" : $"AND {filterFragment.WhereClause}";

            var pivotCteBlock = string.IsNullOrEmpty(attrPivot.CteSql) ? "" : $",\n{attrPivot.CteSql}";
            var withKeySortJoin = string.IsNullOrEmpty(sortResult.WithKeyJoin) ? "" : $"\n    {sortResult.WithKeyJoin}";

            var sql = $@"
;WITH FilteredAssets AS (
    SELECT asd.Id, asd.MaintainableAssetId
    FROM dbo.AssetSummaryDetail asd
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

            return ExecutePagedQuery(sql, parameters, columns, request);
        }

        private PagedViewerResultDTO GetAssetYearlyData(ReportComponent component, ViewerDataRequestDTO request, int runId, string timeScope)
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
                    sc.Field.Equals("cost", StringComparison.OrdinalIgnoreCase)).DisplayHeader, Format = "currency" });
            if (hasBudget)
                columns.Add(new ViewerColumnDTO { Key = "budget", Header = simulationColumns.First(sc =>
                    sc.Field.Equals("budget", StringComparison.OrdinalIgnoreCase)).DisplayHeader, Format = "text" });

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

            var criteriaFragment = TranslateSchemaCriteria(component.CriteriaExpression, "AssetDetailValueIntId", "AssetDetailId", "ad", parameters);
            var filterFragment = TranslateViewerFilters(request.ColumnFilters, "AssetDetailValueIntId", "AssetDetailId", "ad", parameters);
            var sortResult = BuildSortExpression(request.SortBy, request.SortDirection, "AssetDetailValueIntId", "AssetDetailId", "wk", parameters, $"wk._AssetKey, wk.Year");

            var criteriaWhere = criteriaFragment.IsEmpty ? "" : $"AND {criteriaFragment.WhereClause}";
            var filterWhere = filterFragment.IsEmpty ? "" : $"AND {filterFragment.WhereClause}";

            var pivotCteBlock = string.IsNullOrEmpty(attrPivot.CteSql) ? "" : $",\n{attrPivot.CteSql}";
            var costCteBlock = string.IsNullOrEmpty(costBudget.CteSql) ? "" : $",\n{costBudget.CteSql}";
            var withKeySortJoin = string.IsNullOrEmpty(sortResult.WithKeyJoin) ? "" : $"\n    {sortResult.WithKeyJoin}";

            var sql = $@"
;WITH FilteredAssets AS (
    SELECT ad.Id, ad.MaintainableAssetId, syd.Year, ad.AppliedTreatment,
        ({TreatmentCauseCaseExpression("ad")}) AS TreatmentCause,
        ({TreatmentStatusCaseExpression("ad")}) AS TreatmentStatus
    FROM dbo.AssetDetail ad
    INNER JOIN dbo.SimulationYearDetail syd ON syd.RunId = @RunId AND syd.Id = ad.SimulationYearDetailId
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

            return ExecutePagedQuery(sql, parameters, columns, request);
        }

        #endregion

        #region Treatment Timeline

        private PagedViewerResultDTO GetTreatmentTimelineData(ReportComponent component, ViewerDataRequestDTO request, int runId)
        {
            var config = component.Config ?? new Dictionary<string, object>();
            var identifierColumns = GetConfigColumnDefs(config, "identifierColumns");
            var attributeColumns = GetConfigColumnDefs(config, "attributeColumns");

            var allAttrColumns = new List<SimpleColumnDef>();
            allAttrColumns.AddRange(identifierColumns);
            allAttrColumns.AddRange(attributeColumns);

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

            var assetKeyJoin = BuildAssetKeyJoin("fa");
            var attrPivot = BuildAttributePivotCte(allAttrColumns, "AssetDetailValueIntId", "AssetDetailId", parameters, useFallback: true);
            var criteriaFragment = TranslateSchemaCriteria(component.CriteriaExpression, "AssetDetailValueIntId", "AssetDetailId", "ad", parameters);
            var filterFragment = TranslateViewerFilters(request.ColumnFilters, "AssetDetailValueIntId", "AssetDetailId", "ad", parameters);
            var sortResult = BuildSortExpression(request.SortBy, request.SortDirection, "AssetDetailValueIntId", "AssetDetailId", "wk", parameters, $"wk._AssetKey, wk.Year");

            var criteriaWhere = criteriaFragment.IsEmpty ? "" : $"AND {criteriaFragment.WhereClause}";
            var filterWhere = filterFragment.IsEmpty ? "" : $"AND {filterFragment.WhereClause}";

            var pivotCteBlock = string.IsNullOrEmpty(attrPivot.CteSql) ? "" : $",\n{attrPivot.CteSql}";
            var withKeySortJoin = string.IsNullOrEmpty(sortResult.WithKeyJoin) ? "" : $"\n    {sortResult.WithKeyJoin}";

            var sql = $@"
;WITH FilteredAssets AS (
    SELECT ad.Id, ad.MaintainableAssetId, syd.Year, ad.AppliedTreatment,
        ({TreatmentCauseCaseExpression("ad")}) AS TreatmentCause,
        ({TreatmentStatusCaseExpression("ad")}) AS TreatmentStatus
    FROM dbo.AssetDetail ad
    INNER JOIN dbo.SimulationYearDetail syd ON syd.RunId = @RunId AND syd.Id = ad.SimulationYearDetailId
    WHERE ad.RunId = @RunId
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

            return ExecutePagedQuery(sql, parameters, columns, request);
        }

        #endregion

        #region Condition Tracking

        private PagedViewerResultDTO GetConditionTrackingData(ReportComponent component, ViewerDataRequestDTO request, int runId)
        {
            var config = component.Config ?? new Dictionary<string, object>();
            var conditionAttrNames = GetConfigStringArray(config, "conditionAttributes");
            var identifierColumns = GetConfigColumnDefs(config, "identifierColumns");

            var keyAttrInfo = ResolveKeyAttribute(request.SimulationId);
            var keyColName = keyAttrInfo.Name ?? "Asset ID";

            var conditionColumns = conditionAttrNames.Select(name => new SimpleColumnDef
            {
                AttributeName = name,
                DisplayHeader = name,
                Format = "number"
            }).ToList();

            var allAttrColumns = new List<SimpleColumnDef>();
            allAttrColumns.AddRange(identifierColumns);
            allAttrColumns.AddRange(conditionColumns);

            var columns = new List<ViewerColumnDTO>();
            columns.Add(new ViewerColumnDTO { Key = keyColName, Header = keyColName, Format = "text" });
            columns.Add(new ViewerColumnDTO { Key = "year", Header = "Year", Format = "year", Filterable = true });

            foreach (var idCol in identifierColumns)
                columns.Add(new ViewerColumnDTO { Key = idCol.AttributeName, Header = idCol.DisplayHeader, Format = idCol.Format });

            foreach (var condCol in conditionColumns)
                columns.Add(new ViewerColumnDTO { Key = condCol.AttributeName, Header = condCol.DisplayHeader, Format = condCol.Format });

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@RunId", runId),
                new SqlParameter("@Offset", (request.Page - 1) * request.PageSize),
                new SqlParameter("@PageSize", request.PageSize)
            };

            var assetKeyJoin = BuildAssetKeyJoin("fa");
            var attrPivot = BuildAttributePivotCte(allAttrColumns, "AssetDetailValueIntId", "AssetDetailId", parameters, useFallback: true);
            var criteriaFragment = TranslateSchemaCriteria(component.CriteriaExpression, "AssetDetailValueIntId", "AssetDetailId", "ad", parameters);
            var filterFragment = TranslateViewerFilters(request.ColumnFilters, "AssetDetailValueIntId", "AssetDetailId", "ad", parameters);
            var sortResult = BuildSortExpression(request.SortBy, request.SortDirection, "AssetDetailValueIntId", "AssetDetailId", "wk", parameters, $"wk._AssetKey, wk.Year");

            var criteriaWhere = criteriaFragment.IsEmpty ? "" : $"AND {criteriaFragment.WhereClause}";
            var filterWhere = filterFragment.IsEmpty ? "" : $"AND {filterFragment.WhereClause}";

            var pivotCteBlock = string.IsNullOrEmpty(attrPivot.CteSql) ? "" : $",\n{attrPivot.CteSql}";
            var withKeySortJoin = string.IsNullOrEmpty(sortResult.WithKeyJoin) ? "" : $"\n    {sortResult.WithKeyJoin}";

            var sql = $@"
;WITH FilteredAssets AS (
    SELECT ad.Id, ad.MaintainableAssetId, syd.Year
    FROM dbo.AssetDetail ad
    INNER JOIN dbo.SimulationYearDetail syd ON syd.RunId = @RunId AND syd.Id = ad.SimulationYearDetailId
    WHERE ad.RunId = @RunId
    {criteriaWhere}
    {filterWhere}
),
Counted AS (
    SELECT COUNT(*) AS TotalCount FROM FilteredAssets
),
WithKey AS (
    SELECT fa.Id, fa.MaintainableAssetId, fa.Year,
        mal.LocationIdentifier AS _AssetKey
    FROM FilteredAssets fa
    {assetKeyJoin}{withKeySortJoin}
),
SortedAssets AS (
    SELECT wk.*, ROW_NUMBER() OVER (ORDER BY {sortResult.SortColumnRef}) AS RowNum
    FROM WithKey wk
),
PagedRows AS (
    SELECT pg.Id, pg.MaintainableAssetId, pg._AssetKey, pg.Year, pg.RowNum
    FROM SortedAssets pg
    WHERE pg.RowNum BETWEEN @Offset + 1 AND @Offset + @PageSize
){pivotCteBlock}
SELECT pg._AssetKey AS [{keyColName}], pg.Year, c.TotalCount
{attrPivot.SelectColumns}
FROM PagedRows pg
CROSS JOIN Counted c
{attrPivot.JoinClause}
ORDER BY pg.RowNum";

            return ExecutePagedQuery(sql, parameters, columns, request);
        }

        #endregion

        #region Funded Treatment List

        private PagedViewerResultDTO GetFundedTreatmentListData(ReportComponent component, ViewerDataRequestDTO request, int runId)
        {
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
                    sc.Field.Equals("cost", StringComparison.OrdinalIgnoreCase)).DisplayHeader, Format = "currency" });
            if (hasBudget)
                columns.Add(new ViewerColumnDTO { Key = "budget", Header = simulationColumns.First(sc =>
                    sc.Field.Equals("budget", StringComparison.OrdinalIgnoreCase)).DisplayHeader, Format = "text" });

            foreach (var attrCol in attributeColumns)
                columns.Add(new ViewerColumnDTO { Key = attrCol.AttributeName, Header = attrCol.DisplayHeader, Format = attrCol.Format });

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@RunId", runId),
                new SqlParameter("@Offset", (request.Page - 1) * request.PageSize),
                new SqlParameter("@PageSize", request.PageSize)
            };

            var assetKeyJoin = BuildAssetKeyJoin("fa");
            var attrPivot = BuildAttributePivotCte(attributeColumns, "AssetDetailValueIntId", "AssetDetailId", parameters, useFallback: true);
            var costBudget = BuildCostBudgetCte(hasCost, hasBudget);
            var criteriaFragment = TranslateSchemaCriteria(component.CriteriaExpression, "AssetDetailValueIntId", "AssetDetailId", "ad", parameters);
            var filterFragment = TranslateViewerFilters(request.ColumnFilters, "AssetDetailValueIntId", "AssetDetailId", "ad", parameters);
            var sortResult = BuildSortExpression(request.SortBy, request.SortDirection, "AssetDetailValueIntId", "AssetDetailId", "wk", parameters, $"wk._AssetKey, wk.Year");

            var criteriaWhere = criteriaFragment.IsEmpty ? "" : $"AND {criteriaFragment.WhereClause}";
            var filterWhere = filterFragment.IsEmpty ? "" : $"AND {filterFragment.WhereClause}";

            var pivotCteBlock = string.IsNullOrEmpty(attrPivot.CteSql) ? "" : $",\n{attrPivot.CteSql}";
            var costCteBlock = string.IsNullOrEmpty(costBudget.CteSql) ? "" : $",\n{costBudget.CteSql}";
            var withKeySortJoin = string.IsNullOrEmpty(sortResult.WithKeyJoin) ? "" : $"\n    {sortResult.WithKeyJoin}";

            var sql = $@"
;WITH FilteredAssets AS (
    SELECT ad.Id, ad.MaintainableAssetId, syd.Year, ad.AppliedTreatment,
        ({TreatmentCauseCaseExpression("ad")}) AS TreatmentCause,
        ({TreatmentStatusCaseExpression("ad")}) AS TreatmentStatus
    FROM dbo.AssetDetail ad
    INNER JOIN dbo.SimulationYearDetail syd ON syd.RunId = @RunId AND syd.Id = ad.SimulationYearDetailId
    WHERE ad.RunId = @RunId
    AND ad.AppliedTreatment IS NOT NULL AND ad.AppliedTreatment <> 'No Treatment'
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

            return ExecutePagedQuery(sql, parameters, columns, request);
        }

        #endregion

        #region Work Summary

        private PagedViewerResultDTO GetWorkSummaryData(ReportComponent component, ViewerDataRequestDTO request, int runId)
        {
            var config = component.Config ?? new Dictionary<string, object>();
            var groupByAttribute = GetConfigString(config, "groupByAttribute", "");
            var aggregateField = GetConfigString(config, "aggregateField", "count");
            var aggregateFunction = GetConfigString(config, "aggregateFunction", "count");

            // Determine the value column header and format based on aggregate
            string valueHeader;
            string valueFormat;
            if (aggregateField.Equals("cost", StringComparison.OrdinalIgnoreCase))
            {
                valueHeader = aggregateFunction.Equals("count", StringComparison.OrdinalIgnoreCase)
                    ? "Count" : $"{aggregateFunction}(Cost)";
                valueFormat = aggregateFunction.Equals("count", StringComparison.OrdinalIgnoreCase)
                    ? "number" : "currency";
            }
            else
            {
                valueHeader = $"{aggregateFunction}({aggregateField})";
                valueFormat = "number";
            }

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
                    Header = string.IsNullOrEmpty(groupByAttribute) ? "Group" : groupByAttribute,
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
            }

            var rows = new List<List<object>>();

            // Build the groupBy expression using AggregatedResult (which has ALL attributes including static ones)
            // or COALESCE with AssetDetailValueIntId for per-year attributes
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@RunId", runId)
            };

            // Determine whether cost needs to be aggregated
            bool needsCostInCte = aggregateField.Equals("cost", StringComparison.OrdinalIgnoreCase)
                && !aggregateFunction.Equals("count", StringComparison.OrdinalIgnoreCase);

            string aggSelect;
            string groupAttrCtes = "";
            string groupAttrJoins = "";
            string groupValueSelect;

            if (string.IsNullOrEmpty(groupByAttribute))
            {
                groupValueSelect = "'All' AS GroupValue";
            }
            else
            {
                var attr = _unitOfWork.AttributeRepo.GetSingleByName(groupByAttribute);
                if (attr == null)
                {
                    return new PagedViewerResultDTO
                    {
                        Columns = columns,
                        Rows = rows,
                        TotalRowCount = 0,
                        Page = 1,
                        PageSize = 1
                    };
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

            if (needsCostInCte)
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
){groupAttrCtes}{costCte},
AssetData AS (
    SELECT ab.Id, ab.MaintainableAssetId, ab.Year,
        {groupValueSelect}{costColumn}
    FROM AssetBase ab{groupAttrJoins}{costJoin}
)
SELECT ISNULL(adata.GroupValue, '(None)') AS GroupValue, adata.Year, {aggSelect} AS AggValue
FROM AssetData adata
GROUP BY adata.GroupValue, adata.Year
ORDER BY adata.GroupValue, adata.Year";

            using var connection = context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 120;
            foreach (var p in parameters)
                command.Parameters.Add(new SqlParameter(p.ParameterName, p.Value));

            // Read long-format rows from SQL, then pivot to wide format
            // SQL returns: (GroupValue, Year, AggValue) per group×year
            var yearIndexMap = new Dictionary<int, int>();
            for (int yi = 0; yi < years.Count; yi++)
                yearIndexMap[years[yi]] = yi;

            var pivotData = new Dictionary<string, object[]>(StringComparer.OrdinalIgnoreCase);

            using var reader = command.ExecuteReader();
            while (reader.Read())
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

            // Build wide-format rows: [groupValue, y2025_val, y2026_val, ...]
            foreach (var kvp in pivotData.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                var row = new List<object> { kvp.Key };
                foreach (var val in kvp.Value)
                    row.Add(val ?? (object)0);
                rows.Add(row);
            }

            return ApplyInMemoryFilterSortAndPaginate(columns, rows, request);
        }

        #endregion

        #region Budget Summary

        private PagedViewerResultDTO GetBudgetSummaryData(ReportComponent component, ViewerDataRequestDTO request, int runId)
        {
            var config = component.Config ?? new Dictionary<string, object>();
            var showSpent = GetConfigBool(config, "showSpentAmount", true);
            var showRemaining = GetConfigBool(config, "showRemainingAmount", true);

            var context = _unitOfWork.Context;

            // Get the year list for wide-format pivot columns
            var years = context.SimulationYearDetail
                .AsNoTracking()
                .Where(y => y.RunId == runId)
                .OrderBy(y => y.Year)
                .Select(y => y.Year)
                .ToList();

            // Build wide-format columns: budget name + per-year metric columns
            var columns = new List<ViewerColumnDTO>
            {
                new ViewerColumnDTO { Key = "budgetName", Header = "Budget", Format = "text", Filterable = true, Sortable = true }
            };
            // Track column positions per year for the pivot step
            int colsPerYear = (showSpent ? 1 : 0) + (showRemaining ? 1 : 0);
            foreach (var year in years)
            {
                if (showSpent)
                    columns.Add(new ViewerColumnDTO { Key = $"spent_{year}", Header = $"{year} Spent", Format = "currency", Filterable = false, Sortable = false });
                if (showRemaining)
                    columns.Add(new ViewerColumnDTO { Key = $"rem_{year}", Header = $"{year} Remaining", Format = "currency", Filterable = false, Sortable = false });
            }

            // Step 1: Get remaining funding per budget per year from BudgetDetail
            // AvailableFunding = what's LEFT after treatment spending for that year
            var budgetData = context.BudgetDetail
                .AsNoTracking()
                .Where(b => b.RunId == runId)
                .Join(context.SimulationYearDetail.AsNoTracking().Where(y => y.RunId == runId),
                    b => b.SimulationYearDetailId,
                    y => y.Id,
                    (b, y) => new { b.BudgetName, y.Year, b.AvailableFunding })
                .OrderBy(x => x.BudgetName)
                .ThenBy(x => x.Year)
                .ToList();

            // Step 2: Get actual spending per budget per year from Allocation table
            var spendingData = new Dictionary<(string BudgetName, int Year), double>();

            if (showSpent)
            {
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@RunId", runId)
                };

                var sql = @"
SELECT a.BudgetName, a.Year, SUM(CAST(a.AllocatedAmount AS FLOAT)) AS TotalSpent
FROM dbo.Allocation a
WHERE a.RunId = @RunId
GROUP BY a.BudgetName, a.Year";

                using var connection = context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open) connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandTimeout = 120;
                foreach (var p in parameters)
                    command.Parameters.Add(new SqlParameter(p.ParameterName, p.Value));

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var budgetName = reader.GetString(0);
                    var year = reader.GetInt32(1);
                    var totalSpent = reader.IsDBNull(2) ? 0.0 : reader.GetDouble(2);
                    spendingData[(budgetName, year)] = totalSpent;
                }
            }

            // Step 3: Pivot long-format data to wide format
            // Build year index map for O(1) column position lookup
            var yearIndexMap = new Dictionary<int, int>();
            for (int yi = 0; yi < years.Count; yi++)
                yearIndexMap[years[yi]] = yi;

            int totalYearCols = years.Count * colsPerYear;
            var pivotData = new Dictionary<string, object[]>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in budgetData)
            {
                if (!pivotData.TryGetValue(item.BudgetName, out var yearValues))
                {
                    yearValues = new object[totalYearCols];
                    for (int j = 0; j < totalYearCols; j++) yearValues[j] = 0.0;
                    pivotData[item.BudgetName] = yearValues;
                }

                if (yearIndexMap.TryGetValue(item.Year, out int yi))
                {
                    int baseIdx = yi * colsPerYear;
                    int offset = 0;
                    var remaining = (double)item.AvailableFunding;
                    var spent = spendingData.TryGetValue((item.BudgetName, item.Year), out var s) ? s : 0.0;

                    if (showSpent) { yearValues[baseIdx + offset] = spent; offset++; }
                    if (showRemaining) { yearValues[baseIdx + offset] = remaining; }
                }
            }

            // Build wide-format rows: [budgetName, spent_2025, rem_2025, spent_2026, rem_2026, ...]
            var rows = new List<List<object>>();
            foreach (var kvp in pivotData.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                var row = new List<object> { kvp.Key };
                row.AddRange(kvp.Value);
                rows.Add(row);
            }

            return ApplyInMemoryFilterSortAndPaginate(columns, rows, request);
        }

        #endregion

        #region Goal Tracking

        private PagedViewerResultDTO GetGoalTrackingData(ReportComponent component, ViewerDataRequestDTO request, int runId)
        {
            var config = component.Config ?? new Dictionary<string, object>();
            var goalType = GetConfigString(config, "goalType", "target");
            var showGoalName = GetConfigBool(config, "showGoalName", true);
            var showActualVsTarget = GetConfigBool(config, "showActualVsTarget", true);
            var showMetStatus = GetConfigBool(config, "showMetStatus", true);

            var columns = new List<ViewerColumnDTO>();
            if (showGoalName) columns.Add(new ViewerColumnDTO { Key = "goalName", Header = "Goal", Format = "text" });
            columns.Add(new ViewerColumnDTO { Key = "year", Header = "Year", Format = "year" });
            if (showActualVsTarget)
            {
                columns.Add(new ViewerColumnDTO { Key = "actual", Header = "Actual", Format = "number" });
                columns.Add(new ViewerColumnDTO { Key = "target", Header = "Target", Format = "number" });
            }
            if (showMetStatus)
                columns.Add(new ViewerColumnDTO { Key = "goalIsMet", Header = "Met?", Format = "text" });

            var context = _unitOfWork.Context;
            var rows = new List<List<object>>();

            if (goalType == "target")
            {
                var goals = context.TargetConditionGoalDetail
                    .AsNoTracking()
                    .Where(g => g.RunId == runId)
                    .Join(context.SimulationYearDetail.AsNoTracking().Where(y => y.RunId == runId),
                        g => g.SimulationYearDetailId,
                        y => y.Id,
                        (g, y) => new { g.GoalName, y.Year, g.ActualValue, g.TargetValue, g.GoalIsMet })
                    .OrderBy(x => x.GoalName).ThenBy(x => x.Year)
                    .ToList();

                foreach (var g in goals)
                {
                    var row = new List<object>();
                    if (showGoalName) row.Add(g.GoalName ?? "");
                    row.Add(g.Year);
                    if (showActualVsTarget) { row.Add(g.ActualValue); row.Add(g.TargetValue); }
                    if (showMetStatus) row.Add(g.GoalIsMet ? "Yes" : "No");
                    rows.Add(row);
                }
            }
            else
            {
                var goals = context.DeficientConditionGoalDetail
                    .AsNoTracking()
                    .Where(g => g.RunId == runId)
                    .Join(context.SimulationYearDetail.AsNoTracking().Where(y => y.RunId == runId),
                        g => g.SimulationYearDetailId,
                        y => y.Id,
                        (g, y) => new { g.GoalName, y.Year, g.ActualDeficientPercentage, g.AllowedDeficientPercentage, g.GoalIsMet })
                    .OrderBy(x => x.GoalName).ThenBy(x => x.Year)
                    .ToList();

                foreach (var g in goals)
                {
                    var row = new List<object>();
                    if (showGoalName) row.Add(g.GoalName ?? "");
                    row.Add(g.Year);
                    if (showActualVsTarget) { row.Add(g.ActualDeficientPercentage); row.Add(g.AllowedDeficientPercentage); }
                    if (showMetStatus) row.Add(g.GoalIsMet ? "Yes" : "No");
                    rows.Add(row);
                }
            }

            return ApplyInMemoryFilterSortAndPaginate(columns, rows, request);
        }

        #endregion

        #region Text Block

        private PagedViewerResultDTO GetTextBlockData(ReportComponent component, ViewerDataRequestDTO request)
        {
            var config = component.Config ?? new Dictionary<string, object>();
            var content = GetConfigString(config, "content", "");
            var style = GetConfigString(config, "style", "body");

            return new PagedViewerResultDTO
            {
                Columns = new List<ViewerColumnDTO>
                {
                    new ViewerColumnDTO { Key = "content", Header = "Content", Format = "text", Sortable = false, Filterable = false },
                    new ViewerColumnDTO { Key = "style", Header = "Style", Format = "text", Sortable = false, Filterable = false }
                },
                Rows = new List<List<object>> { new List<object> { content, style } },
                TotalRowCount = 1,
                Page = 1,
                PageSize = 1
            };
        }

        #endregion

        #region Helpers

        private int ResolveRunId(Guid simulationId)
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
        private KeyAttributeInfo ResolveKeyAttribute(Guid simulationId)
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
        private static string BuildAssetKeyJoin(string parentAlias = "fa")
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
        private string BuildAttributeSubqueries(
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
        private AttributePivotCteResult BuildAttributePivotCte(
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

            // Build CASE lines shared by both CTEs
            var pivotCases = new StringBuilder();
            foreach (var (col, paramName, isNumber) in resolvedAttrs)
            {
                string valueCol = isNumber ? "NumericValue" : "TextValue";
                pivotCases.AppendLine($"        MAX(CASE WHEN adv.AttributeId = {paramName} THEN adv.{valueCol} END) AS [{col.AttributeName}],");
            }
            // Trim the trailing comma+newline from the last CASE line
            string pivotCasesStr = pivotCases.ToString().TrimEnd().TrimEnd(',');

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
                var summaryPivotCases = new StringBuilder();
                foreach (var (col, paramName, isNumber) in resolvedAttrs)
                {
                    string valueCol = isNumber ? "NumericValue" : "TextValue";
                    summaryPivotCases.AppendLine($"        MAX(CASE WHEN asdv.AttributeId = {paramName} THEN asdv.{valueCol} END) AS [{col.AttributeName}],");
                }
                string summaryPivotCasesStr = summaryPivotCases.ToString().TrimEnd().TrimEnd(',');

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

        private CriteriaSqlFragment TranslateSchemaCriteria(
            string criteriaExpression,
            string valueTableName,
            string parentIdColumn,
            string parentAlias,
            List<SqlParameter> parameters)
        {
            var fragment = _criteriaTranslator.Translate(
                criteriaExpression, _unitOfWork.AttributeRepo,
                valueTableName, parentIdColumn, parentAlias, "sc");

            if (!fragment.IsEmpty)
            {
                foreach (var p in fragment.Parameters)
                {
                    if (!parameters.Any(existing => existing.ParameterName == p.ParameterName))
                        parameters.Add(p);
                }
            }

            return fragment;
        }

        private CriteriaSqlFragment TranslateViewerFilters(
            Dictionary<string, string> columnFilters,
            string valueTableName,
            string parentIdColumn,
            string parentAlias,
            List<SqlParameter> parameters)
        {
            var fragment = _criteriaTranslator.TranslateColumnFilters(
                columnFilters, _unitOfWork.AttributeRepo,
                valueTableName, parentIdColumn, parentAlias,
                AssetDetailSimulationFields);

            if (!fragment.IsEmpty)
            {
                foreach (var p in fragment.Parameters)
                {
                    if (!parameters.Any(existing => existing.ParameterName == p.ParameterName))
                        parameters.Add(p);
                }
            }

            return fragment;
        }

        private SortExpressionResult BuildSortExpression(
            string sortBy,
            string sortDirection,
            string valueTableName,
            string parentIdColumn,
            string parentAlias,
            List<SqlParameter> parameters,
            string defaultSort)
        {
            var direction = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

            if (string.IsNullOrWhiteSpace(sortBy))
                return new SortExpressionResult { SortColumnRef = $"{defaultSort} {direction}" };

            // Check if it's a simulation field — direct column reference, no join needed
            if (AssetDetailSimulationFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            {
                var colName = MapSimFieldToSqlColumn(sortBy);
                return new SortExpressionResult { SortColumnRef = $"{parentAlias}.{colName} {direction}" };
            }

            // EAV attribute sort — pre-compute in WithKey via a set-based subquery join
            var (sortColRef, withKeyJoin) = _criteriaTranslator.BuildAttributeSortExpression(
                sortBy, _unitOfWork.AttributeRepo,
                valueTableName, parentIdColumn, parameters);

            return new SortExpressionResult
            {
                SortColumnRef = $"{sortColRef} {direction}",
                WithKeyJoin = withKeyJoin
            };
        }

        private PagedViewerResultDTO ExecutePagedQuery(
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

        private static string MapColumnKeyToSqlAlias(string key)
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

        private static string MapSimFieldToSqlColumn(string field)
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
        private static CostBudgetCteResult BuildCostBudgetCte(bool hasCost, bool hasBudget)
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
        private static string TreatmentCauseCaseExpression(string alias)
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
        private static string TreatmentStatusCaseExpression(string alias)
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
        /// Used by non-SQL aggregation methods (BudgetSummary, WorkSummary, GoalTracking)
        /// that build all rows in C# and need to filter/sort/page the result before returning.
        /// </summary>
        private static PagedViewerResultDTO ApplyInMemoryFilterSortAndPaginate(
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
        private class ObjectComparer : IComparer<object?>
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

        private static PagedViewerResultDTO EmptyResult(ViewerDataRequestDTO request)
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

        private static string SanitizeParamName(string name)
        {
            return System.Text.RegularExpressions.Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");
        }

        #endregion

        #region Config Helpers

        private static string GetConfigString(Dictionary<string, object> config, string key, string defaultValue = "")
        {
            if (config.TryGetValue(key, out var val) && val != null)
                return val.ToString();
            return defaultValue;
        }

        private static int? GetConfigInt(Dictionary<string, object> config, string key)
        {
            if (config.TryGetValue(key, out var val) && val != null)
            {
                if (val is int i) return i;
                if (val is long l) return (int)l;
                if (int.TryParse(val.ToString(), out var parsed)) return parsed;
            }
            return null;
        }

        private static bool GetConfigBool(Dictionary<string, object> config, string key, bool defaultValue = false)
        {
            if (config.TryGetValue(key, out var val) && val != null)
            {
                if (val is bool b) return b;
                if (bool.TryParse(val.ToString(), out var parsed)) return parsed;
            }
            return defaultValue;
        }

        private static List<SimpleColumnDef> GetConfigColumnDefs(Dictionary<string, object> config, string key)
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

        private static List<SimpleSimulationColumnDef> GetConfigSimulationColumnDefs(Dictionary<string, object> config, string key)
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
        private static List<string> GetConfigStringArray(Dictionary<string, object> config, string key)
        {
            if (!config.TryGetValue(key, out var val) || val == null)
                return new List<string>();

            if (val is JArray jArray)
                return jArray.Select(j => j.ToString()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            return new List<string>();
        }

        #endregion

        #region Internal Types

        internal class SimpleColumnDef
        {
            public string AttributeName { get; set; }
            public string DisplayHeader { get; set; }
            public string Format { get; set; }
        }

        internal class SimpleSimulationColumnDef
        {
            public string Field { get; set; }
            public string DisplayHeader { get; set; }
            public string Format { get; set; }
        }

        internal class KeyAttributeInfo
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
        }

        internal class AttributePivotCteResult
        {
            public string CteSql { get; set; } = "";
            public string JoinClause { get; set; } = "";
            public string SelectColumns { get; set; } = "";
        }

        internal class CostBudgetCteResult
        {
            public string CteSql { get; set; } = "";
            public string JoinClause { get; set; } = "";
            public string CostSelectCol { get; set; } = "";
            public string BudgetSelectCol { get; set; } = "";
        }

        internal class SortExpressionResult
        {
            public string SortColumnRef { get; set; } = "";
            public string WithKeyJoin { get; set; } = "";
        }

        #endregion
    }
}
