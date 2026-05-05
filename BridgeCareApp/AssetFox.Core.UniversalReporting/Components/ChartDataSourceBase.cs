using System.Text;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.UniversalReporting.Charts;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Query;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;
using Microsoft.Data.SqlClient;

namespace AssetFox.Core.UniversalReporting.Components;

public abstract class ChartDataSourceBase : ComponentDataSourceBase
{
    private readonly ChartTemplateValidator _validator;

    protected ChartDataSourceBase(
        UnitOfDataPersistenceWork unitOfWork,
        CriteriaSqlEmitter criteriaEmitter,
        ChartTemplateValidator validator)
        : base(unitOfWork, criteriaEmitter)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    protected ComponentQuery BuildChartQuery(
        ReportComponent component,
        ViewerDataRequest request,
        int runId,
        string expectedKind)
    {
        _validator.ThrowIfInvalid(component);
        var config = ChartConfigReader.Read(component.Config);

        if (!string.Equals(config.ChartKind, expectedKind, StringComparison.OrdinalIgnoreCase))
        {
            config.ChartKind = expectedKind;
        }

        var category = config.Bindings.Category!;
        var value = config.Bindings.Value!;
        var series = config.Bindings.Series;
        var parameters = new List<SqlParameter>
        {
            new("@RunId", runId)
        };

        var usesAllocationBase = string.Equals(value.Source, "allocation", StringComparison.OrdinalIgnoreCase);
        var attributeBindings = new[] { category, value, series }
            .Where(binding => binding != null && string.Equals(binding.Source, "attribute", StringComparison.OrdinalIgnoreCase))
            .GroupBy(binding => binding!.Field, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First()!)
            .ToList();

        var attributes = ResolveAttributes(attributeBindings, parameters);
        var rawBaseSql = BuildRawBaseSql(component, parameters, usesAllocationBase, config);
        var attrCtes = BuildAttributeCtes(attributes);
        var baseCte = BuildBaseCte(attributes);
        var categoryExpr = BuildDimensionExpression(category, attributes);
        var hasSeries = HasBinding(series);
        var seriesExpr = hasSeries ? BuildDimensionExpression(series!, attributes) : "CAST(NULL AS NVARCHAR(200))";
        var filterSql = BuildFilterSql(config, category, series, parameters);
        var groupBySql = hasSeries
            ? $"GROUP BY {categoryExpr}, {seriesExpr}"
            : $"GROUP BY {categoryExpr}";
        var valueExpr = BuildValueExpression(value, attributes);
        var aggregateExpr = BuildAggregateExpression(value.Aggregate, valueExpr);
        var topN = config.Limit.TopN.GetValueOrDefault();
        var hasTopN = topN > 0;
        var orderBySql = BuildOrderBySql(hasSeries, hasTopN || string.Equals(expectedKind, "pie", StringComparison.OrdinalIgnoreCase));
        var topSql = hasTopN
            ? $"TOP ({Math.Min(topN, 100)}) "
            : "";

        var sql = $@"
{rawBaseSql}{attrCtes}{baseCte}
SELECT {topSql}
    {categoryExpr} AS Category,
    {seriesExpr} AS Series,
    {aggregateExpr} AS Value
FROM Base b
{filterSql}
{groupBySql}
{orderBySql}";

        var columns = new List<ViewerColumnDTO>
        {
            new() { Key = "category", Header = category.Label, Format = category.Format, Filterable = false, Sortable = false },
            new() { Key = "series", Header = series?.Label ?? "Series", Format = "text", Filterable = false, Sortable = false },
            new() { Key = "value", Header = value.Label, Format = value.Format, Filterable = false, Sortable = false }
        };

        return CreateQuery(sql, parameters, columns);
    }

    private string BuildRawBaseSql(
        ReportComponent component,
        List<SqlParameter> parameters,
        bool usesAllocationBase,
        ChartConfigDefinition config)
    {
        var criteriaFragment = TranslateSchemaCriteria(
            component.CriteriaExpression,
            "AssetDetailValueIntId",
            "AssetDetailId",
            "ad",
            parameters,
            useSummaryFallback: true);

        var fragmentCtePrefix = BuildFragmentCtePrefix(criteriaFragment);
        var fragmentJoins = BuildFragmentJoins(criteriaFragment);
        var criteriaWhere = criteriaFragment.IsEmpty ? "" : $"AND {criteriaFragment.WhereClause}";
        var yearWhere = BuildYearWhere(config.YearScope, parameters);

        if (usesAllocationBase)
        {
            return $@";WITH {fragmentCtePrefix}RawBase AS (
    SELECT
        ad.Id AS AssetDetailId,
        ad.MaintainableAssetId,
        syd.Year,
        ad.AppliedTreatment,
        alloc.BudgetName,
        COALESCE(alloc.TreatmentName, tcd.TreatmentName, ad.AppliedTreatment) AS TreatmentName,
        CAST(alloc.AllocatedAmount AS FLOAT) AS AllocatedAmount
    FROM dbo.Allocation alloc
    INNER JOIN dbo.FundingCalculationOutput fco
        ON fco.RunId = alloc.RunId AND fco.Id = alloc.FundingCalculationOutputId
    INNER JOIN dbo.TreatmentConsiderationDetail tcd
        ON tcd.RunId = fco.RunId AND tcd.Id = fco.TreatmentConsiderationDetailId
    INNER JOIN dbo.AssetDetail ad
        ON ad.RunId = tcd.RunId AND ad.Id = tcd.AssetDetailId
    INNER JOIN dbo.SimulationYearDetail syd
        ON syd.RunId = ad.RunId AND syd.Id = ad.SimulationYearDetailId{fragmentJoins}
    WHERE alloc.RunId = @RunId
    {criteriaWhere}
    {yearWhere}
)";
        }

        return $@";WITH {fragmentCtePrefix}RawBase AS (
    SELECT
        ad.Id AS AssetDetailId,
        ad.MaintainableAssetId,
        syd.Year,
        ad.AppliedTreatment,
        CAST(NULL AS NVARCHAR(200)) AS BudgetName,
        ad.AppliedTreatment AS TreatmentName,
        CAST(NULL AS FLOAT) AS AllocatedAmount
    FROM dbo.AssetDetail ad
    INNER JOIN dbo.SimulationYearDetail syd
        ON syd.RunId = ad.RunId AND syd.Id = ad.SimulationYearDetailId{fragmentJoins}
    WHERE ad.RunId = @RunId
    {criteriaWhere}
    {yearWhere}
)";
    }

    private static string BuildYearWhere(ChartYearScopeDefinition yearScope, List<SqlParameter> parameters)
    {
        if (string.Equals(yearScope.Mode, "selectedYear", StringComparison.OrdinalIgnoreCase)
            && yearScope.SelectedYear.HasValue)
        {
            parameters.Add(new SqlParameter("@SelectedYear", yearScope.SelectedYear.Value));
            return "AND syd.Year = @SelectedYear";
        }

        if (string.Equals(yearScope.Mode, "range", StringComparison.OrdinalIgnoreCase))
        {
            var clauses = new List<string>();
            if (yearScope.StartYear.HasValue)
            {
                parameters.Add(new SqlParameter("@StartYear", yearScope.StartYear.Value));
                clauses.Add("syd.Year >= @StartYear");
            }

            if (yearScope.EndYear.HasValue)
            {
                parameters.Add(new SqlParameter("@EndYear", yearScope.EndYear.Value));
                clauses.Add("syd.Year <= @EndYear");
            }

            return clauses.Count > 0 ? $"AND {string.Join(" AND ", clauses)}" : "";
        }

        return "";
    }

    private static string BuildFilterSql(
        ChartConfigDefinition config,
        ChartFieldBindingDefinition category,
        ChartFieldBindingDefinition? series,
        List<SqlParameter> parameters)
    {
        if (!config.Filters.ExcludeNoTreatment
            || (!IsTreatmentBinding(category) && !IsTreatmentBinding(series)))
        {
            return "";
        }

        parameters.Add(new SqlParameter("@NoTreatmentName", "No Treatment"));
        var treatmentColumns = new List<string>();
        if (IsAppliedTreatmentBinding(category) || IsAppliedTreatmentBinding(series))
        {
            treatmentColumns.Add("b.AppliedTreatment");
        }

        if (IsTreatmentNameBinding(category) || IsTreatmentNameBinding(series))
        {
            treatmentColumns.Add("b.TreatmentName");
        }

        var clauses = treatmentColumns
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(column => $"({column} IS NULL OR {column} <> @NoTreatmentName)");

        return $"WHERE {string.Join(" AND ", clauses)}";
    }

    private static string BuildOrderBySql(bool hasSeries, bool sortByValueDescending)
    {
        if (sortByValueDescending)
        {
            return hasSeries
                ? "ORDER BY [Value] DESC, [Category], [Series]"
                : "ORDER BY [Value] DESC, [Category]";
        }

        return hasSeries
            ? "ORDER BY [Category], [Series]"
            : "ORDER BY [Category]";
    }

    private List<ChartAttributeBinding> ResolveAttributes(
        List<ChartFieldBindingDefinition> bindings,
        List<SqlParameter> parameters)
    {
        var result = new List<ChartAttributeBinding>();
        for (var i = 0; i < bindings.Count; i++)
        {
            var binding = bindings[i];
            var attribute = _unitOfWork.AttributeRepo.GetSingleByName(binding.Field);
            if (attribute == null)
            {
                throw new InvalidOperationException($"Chart attribute '{binding.Field}' was not found.");
            }

            var parameterName = $"@ChartAttr{i}";
            parameters.Add(new SqlParameter(parameterName, attribute.Id));
            result.Add(new ChartAttributeBinding(binding.Field, parameterName, $"Attr{i}", attribute.Type == "NUMBER"));
        }

        return result;
    }

    private static string BuildAttributeCtes(IReadOnlyList<ChartAttributeBinding> attributes)
    {
        if (attributes.Count == 0)
        {
            return "";
        }

        var sb = new StringBuilder();
        foreach (var attr in attributes)
        {
            sb.Append($@",
{attr.Alias}Detail AS (
    SELECT
        adv.AssetDetailId,
        CAST(adv.NumericValue AS FLOAT) AS NumberValue,
        COALESCE(CAST(adv.NumericValue AS NVARCHAR(200)), adv.TextValue) AS TextValue
    FROM dbo.AssetDetailValueIntId adv
    WHERE adv.RunId = @RunId
      AND adv.AttributeId = {attr.ParameterName}
      AND adv.AssetDetailId IN (SELECT AssetDetailId FROM RawBase)
),
{attr.Alias}Summary AS (
    SELECT
        asd.MaintainableAssetId,
        CAST(asdv.NumericValue AS FLOAT) AS NumberValue,
        COALESCE(CAST(asdv.NumericValue AS NVARCHAR(200)), asdv.TextValue) AS TextValue
    FROM dbo.AssetSummaryDetailValueIntId asdv
    INNER JOIN dbo.AssetSummaryDetail asd
        ON asd.RunId = @RunId AND asd.Id = asdv.AssetSummaryDetailId
    WHERE asdv.RunId = @RunId
      AND asdv.AttributeId = {attr.ParameterName}
      AND asd.MaintainableAssetId IN (SELECT MaintainableAssetId FROM RawBase)
)");
        }

        return sb.ToString();
    }

    private static string BuildBaseCte(IReadOnlyList<ChartAttributeBinding> attributes)
    {
        if (attributes.Count == 0)
        {
            return @",
Base AS (
    SELECT rb.*
    FROM RawBase rb
)";
        }

        var selectCols = new StringBuilder();
        var joins = new StringBuilder();
        foreach (var attr in attributes)
        {
            selectCols.AppendLine($@",
        COALESCE({attr.Alias}d.NumberValue, {attr.Alias}s.NumberValue) AS {attr.Alias}Number,
        COALESCE({attr.Alias}d.TextValue, {attr.Alias}s.TextValue) AS {attr.Alias}Text");
            joins.AppendLine($@"    LEFT JOIN {attr.Alias}Detail {attr.Alias}d ON {attr.Alias}d.AssetDetailId = rb.AssetDetailId
    LEFT JOIN {attr.Alias}Summary {attr.Alias}s ON {attr.Alias}s.MaintainableAssetId = rb.MaintainableAssetId");
        }

        return $@",
Base AS (
    SELECT rb.*{selectCols}
    FROM RawBase rb
{joins})";
    }

    private static string BuildDimensionExpression(
        ChartFieldBindingDefinition binding,
        IReadOnlyList<ChartAttributeBinding> attributes)
    {
        if (string.Equals(binding.Source, "attribute", StringComparison.OrdinalIgnoreCase))
        {
            var attr = FindAttribute(attributes, binding.Field);
            return $"ISNULL(CAST(b.{attr.Alias}Text AS NVARCHAR(200)), '(None)')";
        }

        return (binding.Source.ToLowerInvariant(), binding.Field.ToLowerInvariant()) switch
        {
            ("simulationyear", "year") => "CAST(b.Year AS NVARCHAR(20))",
            ("assetdetail", "appliedtreatment") => "ISNULL(b.AppliedTreatment, '(None)')",
            ("treatment", "treatmentname") => "ISNULL(b.TreatmentName, '(None)')",
            ("allocation", "budgetname") => "ISNULL(b.BudgetName, '(None)')",
            _ => "CAST('(Unsupported)' AS NVARCHAR(200))"
        };
    }

    private static string BuildValueExpression(
        ChartFieldBindingDefinition binding,
        IReadOnlyList<ChartAttributeBinding> attributes)
    {
        if (string.Equals(binding.Source, "attribute", StringComparison.OrdinalIgnoreCase))
        {
            var attr = FindAttribute(attributes, binding.Field);
            return $"b.{attr.Alias}Number";
        }

        return (binding.Source.ToLowerInvariant(), binding.Field.ToLowerInvariant()) switch
        {
            ("allocation", "allocatedamount") => "b.AllocatedAmount",
            ("assetdetail", "assetcount") => "1",
            _ => "1"
        };
    }

    private static string BuildAggregateExpression(string? aggregate, string valueExpr)
    {
        return aggregate?.ToLowerInvariant() switch
        {
            "avg" => $"COALESCE(AVG({valueExpr}), 0)",
            "min" => $"COALESCE(MIN({valueExpr}), 0)",
            "max" => $"COALESCE(MAX({valueExpr}), 0)",
            "count" => "COUNT(*)",
            _ => $"COALESCE(SUM({valueExpr}), 0)"
        };
    }

    private static bool HasBinding(ChartFieldBindingDefinition? binding) =>
        binding != null
        && !string.IsNullOrWhiteSpace(binding.Source)
        && !string.IsNullOrWhiteSpace(binding.Field)
        && !string.IsNullOrWhiteSpace(binding.Label);

    private static bool IsTreatmentBinding(ChartFieldBindingDefinition? binding) =>
        IsAppliedTreatmentBinding(binding) || IsTreatmentNameBinding(binding);

    private static bool IsAppliedTreatmentBinding(ChartFieldBindingDefinition? binding) =>
        binding != null
        && string.Equals(binding.Source, "assetDetail", StringComparison.OrdinalIgnoreCase)
        && string.Equals(binding.Field, "appliedTreatment", StringComparison.OrdinalIgnoreCase);

    private static bool IsTreatmentNameBinding(ChartFieldBindingDefinition? binding) =>
        binding != null
        && string.Equals(binding.Source, "treatment", StringComparison.OrdinalIgnoreCase)
        && string.Equals(binding.Field, "treatmentName", StringComparison.OrdinalIgnoreCase);

    private static ChartAttributeBinding FindAttribute(
        IReadOnlyList<ChartAttributeBinding> attributes,
        string field)
    {
        var attr = attributes.FirstOrDefault(item => string.Equals(item.Field, field, StringComparison.OrdinalIgnoreCase));
        if (attr == null)
        {
            throw new InvalidOperationException($"Chart attribute '{field}' was not resolved.");
        }

        return attr;
    }

    private sealed record ChartAttributeBinding(
        string Field,
        string ParameterName,
        string Alias,
        bool IsNumber);
}
