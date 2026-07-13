using System.Text;
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
    public class TreatmentTimelineDataSource : ComponentDataSourceBase, IComponentDataSource
    {
        public TreatmentTimelineDataSource(UnitOfDataPersistenceWork unitOfWork, CriteriaSqlEmitter criteriaEmitter)
            : base(unitOfWork, criteriaEmitter)
        {
        }

        public string ComponentType => "treatmentTimeline";

        public ComponentQuery Build(
            ReportExecutionContext executionContext,
            ViewerDataRequest request,
            int runId,
            AttributeLookup attributes) => GetTreatmentTimelineData(executionContext, request, runId);

        private ComponentQuery GetTreatmentTimelineData(ReportExecutionContext executionContext, ViewerDataRequest request, int runId)
        {
            var component = executionContext.Component;
            var config = component.Config ?? new Dictionary<string, object>();
            var staticAttributeColumns = GetConfigColumnDefs(config, "attributeColumns");
            var yearFields = GetYearFields(config);
            var showNoTreatment = GetConfigBool(config, "showNoTreatment");

            var parameters = new List<SqlParameter>
            {
                new("@RunId", runId),
                new("@Offset", (request.Page - 1) * request.PageSize),
                new("@PageSize", request.PageSize)
            };

            var requestedAttributeNames = staticAttributeColumns
                .Where(column => !column.IsCalculatedField)
                .Select(column => column.AttributeName)
                .Concat(yearFields
                    .Where(field => field.Type == "attribute")
                    .Select(field => field.AttributeName))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var resolvedAttributes = ResolveAttributes(requestedAttributeNames, parameters);
            var staticCalculatedFields = BuildCalculatedFieldProjection(
                executionContext,
                staticAttributeColumns
                    .Where(column => column.IsCalculatedField)
                    .Select(column => column.FieldId),
                ReportCalculatedFieldRowScope.SummaryByMaintainableAsset,
                parameters,
                "timelineStatic");
            var yearCalculatedFields = BuildCalculatedFieldProjection(
                executionContext,
                yearFields
                    .Where(field => field.Type == "calculatedField")
                    .Select(field => field.FieldId),
                ReportCalculatedFieldRowScope.Yearly,
                parameters,
                "timelineYear");

            var keyAttrInfo = ResolveKeyAttribute(request.SimulationId);
            var keyColName = keyAttrInfo.Name ?? "Asset ID";
            var columns = new List<ViewerColumnDTO>
            {
                new() { Key = keyColName, Header = keyColName, Format = "text" },
                new() { Key = "year", Header = "Year", Format = "year", Filterable = true }
            };

            var staticOutputs = new List<TimelineAttributeOutput>();
            foreach (var staticColumn in staticAttributeColumns)
            {
                if (staticColumn.IsCalculatedField)
                {
                    var calculatedField = staticCalculatedFields.FindById(staticColumn.FieldId);
                    if (calculatedField == null)
                    {
                        continue;
                    }

                    var calculatedOutputAlias = $"staticAttribute_{staticOutputs.Count}";
                    staticOutputs.Add(new TimelineAttributeOutput(null, calculatedField, calculatedOutputAlias));
                    columns.Add(new ViewerColumnDTO
                    {
                        Key = calculatedOutputAlias,
                        Header = staticColumn.DisplayHeader,
                        Format = staticColumn.Format,
                        Sortable = false,
                        Filterable = false
                    });
                    continue;
                }

                if (!resolvedAttributes.TryGetValue(staticColumn.AttributeName, out var attribute))
                {
                    continue;
                }

                var outputAlias = $"staticAttribute_{staticOutputs.Count}";
                staticOutputs.Add(new TimelineAttributeOutput(attribute, null, outputAlias));
                columns.Add(new ViewerColumnDTO
                {
                    Key = outputAlias,
                    Header = staticColumn.DisplayHeader,
                    Format = staticColumn.Format,
                    Sortable = false,
                    Filterable = false
                });
            }

            var yearOutputs = new List<TimelineYearOutput>();
            foreach (var field in yearFields)
            {
                switch (field.Type)
                {
                    case "treatment":
                        yearOutputs.Add(new TimelineYearOutput(field, "appliedTreatment", null));
                        columns.Add(new ViewerColumnDTO
                        {
                            Key = "appliedTreatment",
                            Header = field.DisplayHeader,
                            Format = field.Format
                        });
                        break;
                    case "cost":
                        yearOutputs.Add(new TimelineYearOutput(field, "cost", null));
                        columns.Add(new ViewerColumnDTO
                        {
                            Key = "cost",
                            Header = field.DisplayHeader,
                            Format = field.Format,
                            Sortable = false,
                            Filterable = false
                        });
                        break;
                    case "attribute" when resolvedAttributes.TryGetValue(field.AttributeName, out var attribute):
                        var outputAlias = $"yearAttribute_{yearOutputs.Count}";
                        yearOutputs.Add(new TimelineYearOutput(field, outputAlias, attribute));
                        columns.Add(new ViewerColumnDTO
                        {
                            Key = outputAlias,
                            Header = field.DisplayHeader,
                            Format = field.Format,
                            Sortable = false,
                            Filterable = false
                        });
                        break;
                    case "calculatedField":
                        var calculatedField = yearCalculatedFields.FindById(field.FieldId);
                        if (calculatedField == null)
                        {
                            break;
                        }

                        var calculatedOutputAlias = $"yearCalculated_{yearOutputs.Count}";
                        yearOutputs.Add(new TimelineYearOutput(field, calculatedOutputAlias, null, calculatedField));
                        columns.Add(new ViewerColumnDTO
                        {
                            Key = calculatedOutputAlias,
                            Header = field.DisplayHeader,
                            Format = field.Format,
                            Sortable = false,
                            Filterable = false
                        });
                        break;
                }
            }

            var noTreatmentWhere = "";
            if (!showNoTreatment)
            {
                parameters.Add(new SqlParameter("@NoTreatmentName", "No Treatment"));
                noTreatmentWhere = "AND (ad.AppliedTreatment IS NULL OR ad.AppliedTreatment <> @NoTreatmentName)";
            }

            var assetKeyJoin = BuildAssetKeyJoin("fa");
            var criteriaFragment = TranslateSchemaCriteria(
                component.CriteriaExpression,
                "AssetDetailValueIntId",
                "AssetDetailId",
                "ad",
                parameters,
                useSummaryFallback: true);
            var filterFragment = TranslateViewerFilters(
                request.ColumnFilters,
                "AssetDetailValueIntId",
                "AssetDetailId",
                "ad",
                parameters,
                useSummaryFallback: true,
                assetKeyColumnName: keyColName);
            var sortResult = BuildSortExpression(
                request.SortBy,
                request.SortDirection,
                "AssetDetailValueIntId",
                "AssetDetailId",
                "wk",
                parameters,
                $"wk._AssetKey, wk.Year",
                useSummaryFallback: true,
                assetKeyColumnName: keyColName);

            var fragmentCtePrefix = BuildFragmentCtePrefix(criteriaFragment, filterFragment);
            var fragmentJoins = BuildFragmentJoins(criteriaFragment, filterFragment);
            var criteriaWhere = criteriaFragment.IsEmpty ? "" : $"AND {criteriaFragment.WhereClause}";
            var filterWhere = filterFragment.IsEmpty ? "" : $"AND {filterFragment.WhereClause}";
            var withKeySortJoin = string.IsNullOrEmpty(sortResult.WithKeyJoin) ? "" : $"\n    {sortResult.WithKeyJoin}";

            var yearAttributeNames = yearOutputs
                .Where(output => output.Attribute != null)
                .Select(output => output.Attribute!.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var attributeCtes = BuildAttributeCtes(resolvedAttributes.Values.ToList(), yearAttributeNames);
            var hasCost = yearOutputs.Any(output => output.Field.Type == "cost");
            var costData = BuildCostBudgetCte(hasCost, hasBudget: false);
            var costCteBlock = string.IsNullOrEmpty(costData.CteSql) ? "" : $",\n{costData.CteSql}";

            var outputSelect = new StringBuilder();
            foreach (var output in staticOutputs)
            {
                if (output.Attribute != null)
                {
                    outputSelect.AppendLine($"    , tsa.[{output.Attribute.SqlAlias}] AS [{output.OutputAlias}]");
                }
                else if (output.CalculatedField != null)
                {
                    outputSelect.AppendLine($"    , tscfv.[{output.CalculatedField.ColumnKey}] AS [{output.OutputAlias}]");
                }
            }

            foreach (var output in yearOutputs)
            {
                if (output.Field.Type == "treatment")
                {
                    outputSelect.AppendLine("    , pg.AppliedTreatment AS AppliedTreatment");
                }
                else if (output.Field.Type == "cost")
                {
                    outputSelect.AppendLine(costData.CostSelectCol);
                }
                else if (output.Attribute != null)
                {
                    outputSelect.AppendLine(
                        $"    , COALESCE(tya.[{output.Attribute.SqlAlias}], tsa.[{output.Attribute.SqlAlias}]) AS [{output.OutputAlias}]");
                }
                else if (output.CalculatedField != null)
                {
                    outputSelect.AppendLine($"    , tycfv.[{output.CalculatedField.ColumnKey}] AS [{output.OutputAlias}]");
                }
            }

            var calculatedCtes = new[] { staticCalculatedFields.CteSql, yearCalculatedFields.CteSql }
                .Where(cte => !string.IsNullOrWhiteSpace(cte))
                .ToList();
            var calculatedCtePrefix = calculatedCtes.Count == 0
                ? ""
                : $"{string.Join(",\n", calculatedCtes)},\n";
            var staticCalculatedJoin = staticCalculatedFields.IsEmpty
                ? ""
                : $"\n{staticCalculatedFields.Join("pg", "MaintainableAssetId", "tscfv")}";
            var yearCalculatedJoin = yearCalculatedFields.IsEmpty
                ? ""
                : $"\n{yearCalculatedFields.Join("pg", "Id", "tycfv")}";

            var sql = $@"
;WITH {fragmentCtePrefix}{calculatedCtePrefix}FilteredAssets AS (
    SELECT ad.Id, ad.MaintainableAssetId, syd.Year, ad.AppliedTreatment
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
    SELECT fa.Id, fa.MaintainableAssetId, fa.Year, fa.AppliedTreatment,
        mal.LocationIdentifier AS _AssetKey
    FROM FilteredAssets fa
    {assetKeyJoin}{withKeySortJoin}
),
SortedAssets AS (
    SELECT wk.*, ROW_NUMBER() OVER (ORDER BY {sortResult.SortColumnRef}) AS RowNum
    FROM WithKey wk
),
PagedRows AS (
    SELECT pg.Id, pg.MaintainableAssetId, pg._AssetKey, pg.Year, pg.AppliedTreatment, pg.RowNum
    FROM SortedAssets pg
    WHERE pg.RowNum BETWEEN @Offset + 1 AND @Offset + @PageSize
){attributeCtes.CteBlock}{costCteBlock}
SELECT pg._AssetKey AS [{keyColName}], pg.Year, c.TotalCount
{outputSelect.ToString().TrimEnd()}
FROM PagedRows pg
CROSS JOIN Counted c
{attributeCtes.JoinBlock}
{staticCalculatedJoin}{yearCalculatedJoin}
{costData.JoinClause}
ORDER BY pg.RowNum";

            return CreateQuery(sql, parameters, columns);
        }

        private Dictionary<string, ResolvedTimelineAttribute> ResolveAttributes(
            IReadOnlyList<string> attributeNames,
            List<SqlParameter> parameters)
        {
            var resolved = new Dictionary<string, ResolvedTimelineAttribute>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in attributeNames)
            {
                var attribute = _unitOfWork.AttributeRepo.GetSingleByName(name);
                if (attribute == null)
                {
                    continue;
                }

                var index = resolved.Count;
                var parameterName = $"@timelineAttr_{index}";
                parameters.Add(new SqlParameter(parameterName, attribute.Id));
                resolved[name] = new ResolvedTimelineAttribute(
                    name,
                    $"Attr{index}",
                    parameterName,
                    attribute.Type == "NUMBER");
            }

            return resolved;
        }

        private static TimelineAttributeCtes BuildAttributeCtes(
            IReadOnlyList<ResolvedTimelineAttribute> attributes,
            IReadOnlySet<string> yearAttributeNames)
        {
            var ctes = new List<string>();
            var joins = new List<string>();
            var yearAttributes = attributes
                .Where(attribute => yearAttributeNames.Contains(attribute.Name))
                .ToList();

            if (yearAttributes.Count > 0)
            {
                ctes.Add($@"TimelineYearAttrs AS (
    SELECT adv.AssetDetailId,
{BuildPivotCases(yearAttributes, "adv")}
    FROM dbo.AssetDetailValueIntId adv
    WHERE adv.RunId = @RunId
      AND adv.AssetDetailId IN (SELECT Id FROM PagedRows)
    GROUP BY adv.AssetDetailId
)");
                joins.Add("LEFT JOIN TimelineYearAttrs tya ON tya.AssetDetailId = pg.Id");
            }

            if (attributes.Count > 0)
            {
                ctes.Add($@"TimelineSummaryAttrs AS (
    SELECT asd.MaintainableAssetId,
{BuildPivotCases(attributes, "asdv")}
    FROM dbo.AssetSummaryDetailValueIntId asdv
    INNER JOIN dbo.AssetSummaryDetail asd
        ON asd.RunId = @RunId AND asd.Id = asdv.AssetSummaryDetailId
    WHERE asdv.RunId = @RunId
      AND asd.MaintainableAssetId IN (SELECT MaintainableAssetId FROM PagedRows)
    GROUP BY asd.MaintainableAssetId
)");
                joins.Add("LEFT JOIN TimelineSummaryAttrs tsa ON tsa.MaintainableAssetId = pg.MaintainableAssetId");
            }

            return new TimelineAttributeCtes(
                ctes.Count == 0 ? "" : $",\n{string.Join(",\n", ctes)}",
                string.Join("\n", joins));
        }

        private static string BuildPivotCases(
            IReadOnlyList<ResolvedTimelineAttribute> attributes,
            string tableAlias) => string.Join(",\n", attributes.Select(attribute =>
                $"        MAX(CASE WHEN {tableAlias}.AttributeId = {attribute.ParameterName} THEN {tableAlias}.{(attribute.IsNumber ? "NumericValue" : "TextValue")} END) AS [{attribute.SqlAlias}]"));

        private static List<TimelineYearField> GetYearFields(Dictionary<string, object> config)
        {
            if (!config.TryGetValue("yearDataFields", out var value) || value == null)
            {
                return DefaultYearFields();
            }

            if (value is not JArray fields)
            {
                return DefaultYearFields();
            }

            var results = new List<TimelineYearField>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in fields)
            {
                var type = field["type"]?.ToString()?.Trim().ToLowerInvariant() ?? "";
                var attributeName = field["attributeName"]?.ToString()?.Trim() ?? "";
                var fieldId = field["fieldId"]?.ToString()?.Trim() ?? "";
                if (type is not ("treatment" or "cost" or "attribute" or "calculatedfield")
                    || (type == "attribute" && string.IsNullOrWhiteSpace(attributeName))
                    || (type == "calculatedfield" && !Guid.TryParse(fieldId, out _)))
                {
                    continue;
                }

                type = type == "calculatedfield" ? "calculatedField" : type;
                var identity = type switch
                {
                    "attribute" => $"attribute:{attributeName}",
                    "calculatedField" => $"calculatedField:{fieldId}",
                    _ => type
                };
                if (!seen.Add(identity))
                {
                    continue;
                }

                var defaultHeader = type switch
                {
                    "treatment" => "Treatment",
                    "cost" => "Cost",
                    "calculatedField" => field["displayHeader"]?.ToString() ?? "Calculated Field",
                    _ => attributeName
                };
                var defaultFormat = type == "cost" ? "currency" : "text";
                var displayHeader = field["displayHeader"]?.ToString();
                var format = field["format"]?.ToString();
                results.Add(new TimelineYearField(
                    type,
                    attributeName,
                    fieldId,
                    string.IsNullOrWhiteSpace(displayHeader) ? defaultHeader : displayHeader,
                    string.IsNullOrWhiteSpace(format) ? defaultFormat : format));
            }

            return results;
        }

        private static List<TimelineYearField> DefaultYearFields() => new()
        {
            new("treatment", "", "", "Treatment", "text"),
            new("cost", "", "", "Cost", "currency")
        };

        private sealed record ResolvedTimelineAttribute(
            string Name,
            string SqlAlias,
            string ParameterName,
            bool IsNumber);

        private sealed record TimelineAttributeOutput(
            ResolvedTimelineAttribute? Attribute,
            ReportCalculatedFieldProjection? CalculatedField,
            string OutputAlias);

        private sealed record TimelineYearField(
            string Type,
            string AttributeName,
            string FieldId,
            string DisplayHeader,
            string Format);

        private sealed record TimelineYearOutput(
            TimelineYearField Field,
            string OutputAlias,
            ResolvedTimelineAttribute? Attribute,
            ReportCalculatedFieldProjection? CalculatedField = null);

        private sealed record TimelineAttributeCtes(string CteBlock, string JoinBlock);
    }
}
