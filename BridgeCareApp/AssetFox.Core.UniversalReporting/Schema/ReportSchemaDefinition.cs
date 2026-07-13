using Newtonsoft.Json;

namespace AssetFox.Core.UniversalReporting.Schema;

public class ReportSchemaDefinition
{
    public string SchemaVersion { get; set; } = "1.1";
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<ReportCalculatedField> CalculatedFields { get; set; } = new();
    public List<ReportPage> Pages { get; set; } = new();

    /// <summary>
    /// Returns all components across all pages.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<ReportComponent> AllComponents =>
        Pages?.SelectMany(p => p.Components ?? Enumerable.Empty<ReportComponent>())
        ?? Enumerable.Empty<ReportComponent>();
}
