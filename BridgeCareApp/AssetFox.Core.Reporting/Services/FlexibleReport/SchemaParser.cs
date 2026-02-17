using System;
using AssetFox.Core.Reporting.Models.FlexibleReport;
using Newtonsoft.Json;

namespace AssetFox.Core.Reporting.Services.FlexibleReport
{
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

            return schema;
        }

        public static bool TryParse(string schemaJson, out ReportSchemaDefinition schema, out string error)
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
}
