namespace AssetFox.Core.UniversalReporting.Viewer;

public class ViewerDataRequest
{
    public Guid SimulationId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SortBy { get; set; }
    public string SortDirection { get; set; } = "asc";
    public Dictionary<string, string> ColumnFilters { get; set; } = new();
}
