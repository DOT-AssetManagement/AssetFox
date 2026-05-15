namespace AssetFox.Core.UniversalReporting.Viewer;

public class PagedViewerResult
{
    public List<ViewerColumn> Columns { get; set; } = new();
    public List<List<object>> Rows { get; set; } = new();
    public int TotalRowCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalRowCount / PageSize) : 0;
}
