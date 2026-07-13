using Newtonsoft.Json;

namespace AssetFox.Core.UniversalReporting.Schema;

public static class SchemaParser
{
    public static ReportSchemaDefinition Parse(string schemaJson)
    {
        if (string.IsNullOrWhiteSpace(schemaJson))
        {
            throw new ArgumentException("Schema JSON is null or empty.", nameof(schemaJson));
        }

        var schema = JsonConvert.DeserializeObject<ReportSchemaDefinition>(schemaJson);
        if (schema == null)
        {
            throw new InvalidOperationException("Failed to deserialize report schema JSON.");
        }

        schema.CalculatedFields ??= new List<ReportCalculatedField>();
        schema.Pages ??= new List<ReportPage>();
        foreach (var field in schema.CalculatedFields)
        {
            field.Rules ??= new List<ReportCalculationRule>();
            foreach (var rule in field.Rules)
            {
                rule.Conditions ??= new List<ReportRuleCondition>();
            }
        }

        return schema;
    }

    public static bool TryParse(string schemaJson, out ReportSchemaDefinition? schema, out string? error)
    {
        schema = null;
        error = null;

        try
        {
            schema = Parse(schemaJson);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
