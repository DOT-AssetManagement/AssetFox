using AssetFox.Core.UniversalReporting.Schema;

namespace AssetFox.Core.UniversalReporting.Charts;

public class ChartTemplateValidator
{
    public static bool IsChartComponent(string? componentType) =>
        string.Equals(componentType, "barChart", StringComparison.OrdinalIgnoreCase)
        || string.Equals(componentType, "pieChart", StringComparison.OrdinalIgnoreCase)
        || string.Equals(componentType, "lineChart", StringComparison.OrdinalIgnoreCase);

    public IReadOnlyList<ChartValidationIssue> ValidateSchema(ReportSchemaDefinition schema)
    {
        var issues = new List<ChartValidationIssue>();
        foreach (var component in schema.AllComponents.Where(component => IsChartComponent(component.Type)))
        {
            issues.AddRange(ValidateComponent(component));
        }

        return issues;
    }

    public IReadOnlyList<ChartValidationIssue> ValidateComponent(ReportComponent component)
    {
        var config = ChartConfigReader.Read(component.Config);
        var issues = new List<ChartValidationIssue>();
        var componentId = component.Id ?? "";

        void Add(string message, bool blocker = true) =>
            issues.Add(new ChartValidationIssue
            {
                ComponentId = componentId,
                Message = message,
                Blocker = blocker
            });

        var chartKind = ResolveChartKind(component.Type, config.ChartKind);
        var category = config.Bindings.Category;
        var value = config.Bindings.Value;
        var series = config.Bindings.Series;

        if (!HasBinding(category))
        {
            Add("Choose a category field.");
        }

        if (!HasBinding(value))
        {
            Add("Choose a value field.");
        }
        else if (!string.Equals(value!.Role, "measure", StringComparison.OrdinalIgnoreCase))
        {
            Add($"{BindingLabel(value)} must be used as a measure.");
        }

        if (string.Equals(chartKind, "pie", StringComparison.OrdinalIgnoreCase) && HasBinding(series))
        {
            Add("Pie charts do not support a series split.");
        }

        if (config.Limit.TopN.HasValue
            && (config.Limit.TopN.Value < 1 || config.Limit.TopN.Value > 100))
        {
            Add("Top N must be between 1 and 100.");
        }

        if (value != null && category != null && !CompatibleWithValue(value, category))
        {
            Add($"{BindingLabel(category)} cannot be joined to {BindingLabel(value)}.");
        }

        if (value != null && series != null && !CompatibleWithValue(value, series))
        {
            Add($"{BindingLabel(series)} cannot be joined to {BindingLabel(value)}.");
        }

        return issues;
    }

    public void ThrowIfInvalid(ReportComponent component)
    {
        var blockers = ValidateComponent(component)
            .Where(issue => issue.Blocker)
            .Select(issue => issue.Message)
            .ToList();

        if (blockers.Count > 0)
        {
            throw new InvalidOperationException(
                $"Chart component '{component.Title ?? component.Id}' is invalid: {string.Join(" ", blockers)}");
        }
    }

    private static string ResolveChartKind(string? componentType, string? configKind)
    {
        if (!string.IsNullOrWhiteSpace(configKind))
        {
            return configKind;
        }

        return componentType?.ToLowerInvariant() switch
        {
            "piechart" => "pie",
            "linechart" => "line",
            _ => "bar"
        };
    }

    private static bool HasBinding(ChartFieldBindingDefinition? binding) =>
        binding != null
        && !string.IsNullOrWhiteSpace(binding.Source)
        && !string.IsNullOrWhiteSpace(binding.Field)
        && !string.IsNullOrWhiteSpace(binding.Label);

    private static string BindingLabel(ChartFieldBindingDefinition binding) =>
        string.IsNullOrWhiteSpace(binding.Label) ? binding.Field : binding.Label;

    private static bool CompatibleWithValue(
        ChartFieldBindingDefinition value,
        ChartFieldBindingDefinition binding)
    {
        if (string.Equals(value.Source, "allocation", StringComparison.OrdinalIgnoreCase))
        {
            return SourceIn(binding.Source, "allocation", "simulationYear", "treatment", "assetDetail", "attribute");
        }

        if (string.Equals(value.Source, "assetDetail", StringComparison.OrdinalIgnoreCase))
        {
            return SourceIn(binding.Source, "simulationYear", "assetDetail", "attribute");
        }

        if (string.Equals(value.Source, "attribute", StringComparison.OrdinalIgnoreCase))
        {
            return SourceIn(binding.Source, "simulationYear", "assetDetail", "attribute");
        }

        return false;
    }

    private static bool SourceIn(string? source, params string[] allowed) =>
        allowed.Any(item => string.Equals(item, source, StringComparison.OrdinalIgnoreCase));
}
