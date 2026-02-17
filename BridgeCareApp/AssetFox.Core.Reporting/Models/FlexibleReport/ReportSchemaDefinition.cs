using System.Collections.Generic;

namespace AssetFox.Core.Reporting.Models.FlexibleReport
{
    public class ReportSchemaDefinition
    {
        public string SchemaVersion { get; set; } = "2.0";
        public string Name { get; set; }
        public string Description { get; set; }
        public PageSettings PageSettings { get; set; }
        public List<ReportComponent> Components { get; set; } = new();
    }

    public class PageSettings
    {
        public string Orientation { get; set; } = "landscape";
        public string PaperSize { get; set; } = "letter";
    }

    public class ReportComponent
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public bool Visible { get; set; } = true;
        public string ExcelTabName { get; set; }
        public string CriteriaExpression { get; set; }
        public Dictionary<string, object> Config { get; set; } = new();
    }
}
