namespace AssetFox.Core.UniversalReporting.Schema;

public class ReportComponent
{
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int Order { get; set; }
    public bool Visible { get; set; } = true;
    public string? CriteriaExpression { get; set; }

    public int PageSize { get; set; } = 10;
    public bool StripedRows { get; set; }

    // Grid layout
    public int GridX { get; set; }
    public int GridY { get; set; }
    public int GridW { get; set; } = 12;
    public int GridH { get; set; } = 4;

    public Dictionary<string, object> Config { get; set; } = new();
}
