using Newtonsoft.Json.Linq;

namespace AssetFox.Core.UniversalReporting.Charts;

public sealed class ChartConfigDefinition
{
    public string ChartKind { get; set; } = "bar";
    public string Mode { get; set; } = "single";
    public ChartBindingsDefinition Bindings { get; set; } = new();
    public ChartYearScopeDefinition YearScope { get; set; } = new();
    public ChartLimitDefinition Limit { get; set; } = new();
    public ChartFilterDefinition Filters { get; set; } = new();
    public ChartDisplayDefinition Display { get; set; } = new();
    public ChartValueStylingDefinition? ValueStyling { get; set; }
}

public sealed class ChartValueStylingDefinition
{
    public string BindingKey { get; set; } = "";
    public string Order { get; set; } = "listed";
    public List<ChartValueStyleDefinition> Values { get; set; } = new();
}

public sealed class ChartValueStyleDefinition
{
    public string Value { get; set; } = "";
    public string Color { get; set; } = "";
}

public sealed class ChartBindingsDefinition
{
    public ChartFieldBindingDefinition? Category { get; set; }
    public ChartFieldBindingDefinition? Value { get; set; }
    public ChartFieldBindingDefinition? Series { get; set; }
}

public sealed class ChartFieldBindingDefinition
{
    public string Source { get; set; } = "";
    public string Field { get; set; } = "";
    public string Label { get; set; } = "";
    public string Role { get; set; } = "";
    public string? Aggregate { get; set; }
    public string Format { get; set; } = "text";
}

public sealed class ChartYearScopeDefinition
{
    public string Mode { get; set; } = "allYears";
    public int? SelectedYear { get; set; }
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
}

public sealed class ChartLimitDefinition
{
    public int? TopN { get; set; }
    public bool IncludeOther { get; set; }
}

public sealed class ChartFilterDefinition
{
    public bool ExcludeNoTreatment { get; set; }
}

public sealed class ChartDisplayDefinition
{
    public bool ShowLegend { get; set; } = true;
    public string LegendPosition { get; set; } = "right";
    public bool ShowLabels { get; set; }
    public bool ShowGridLines { get; set; } = true;
    public bool Stacked { get; set; }
    public bool Horizontal { get; set; }
    public bool ShowMarkers { get; set; }
    public bool Donut { get; set; }
}

public sealed class ChartValidationIssue
{
    public string ComponentId { get; set; } = "";
    public string Message { get; set; } = "";
    public bool Blocker { get; set; } = true;
}

public static class ChartConfigReader
{
    public static ChartConfigDefinition Read(Dictionary<string, object>? config)
    {
        if (config == null || config.Count == 0)
        {
            return new ChartConfigDefinition();
        }

        var json = JObject.FromObject(config);
        var definition = json.ToObject<ChartConfigDefinition>() ?? new ChartConfigDefinition();
        definition.Bindings ??= new ChartBindingsDefinition();
        definition.YearScope ??= new ChartYearScopeDefinition();
        definition.Limit ??= new ChartLimitDefinition();
        definition.Filters ??= new ChartFilterDefinition();
        definition.Display ??= new ChartDisplayDefinition();
        if (definition.ValueStyling != null)
        {
            definition.ValueStyling.Values ??= new List<ChartValueStyleDefinition>();
        }

        return definition;
    }
}
