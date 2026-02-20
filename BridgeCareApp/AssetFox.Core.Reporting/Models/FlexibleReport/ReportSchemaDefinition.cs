using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AssetFox.Core.Reporting.Models.FlexibleReport
{
    public class ReportSchemaDefinition
    {
        public string SchemaVersion { get; set; } = "3.0";
        public string Name { get; set; }
        public string Description { get; set; }
        public PageSettings PageSettings { get; set; }
        public List<ReportPage> Pages { get; set; } = new();

        /// <summary>
        /// Returns all components across all pages.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<ReportComponent> AllComponents =>
            Pages?.SelectMany(p => p.Components ?? Enumerable.Empty<ReportComponent>())
            ?? Enumerable.Empty<ReportComponent>();
    }

    public class ReportPage
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
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

        // Grid layout
        public int GridX { get; set; }
        public int GridY { get; set; }
        public int GridW { get; set; } = 12;
        public int GridH { get; set; } = 4;

        public Dictionary<string, object> Config { get; set; } = new();
    }
}
