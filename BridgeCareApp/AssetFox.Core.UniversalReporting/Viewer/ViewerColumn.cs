namespace AssetFox.Core.UniversalReporting.Viewer;

public class ViewerColumn
{
    public string? Key { get; set; }
    public string? Header { get; set; }
    public string Format { get; set; } = "text";
    public bool Sortable { get; set; } = true;
    public bool Filterable { get; set; } = true;
}
