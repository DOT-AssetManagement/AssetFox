namespace AssetFox.Core.UniversalReporting.Schema;

public class ReportPage
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public int Order { get; set; }
    public List<ReportComponent> Components { get; set; } = new();
}
