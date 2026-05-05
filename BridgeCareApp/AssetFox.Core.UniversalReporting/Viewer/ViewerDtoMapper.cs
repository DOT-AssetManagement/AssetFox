using AssetFox.Core.DTOs;

namespace AssetFox.Core.UniversalReporting.Viewer;

public static class ViewerDtoMapper
{
    public static ViewerDataRequest FromDto(ViewerDataRequestDTO dto) =>
        new()
        {
            SimulationId = dto.SimulationId,
            Page = dto.Page,
            PageSize = dto.PageSize,
            SortBy = dto.SortBy,
            SortDirection = dto.SortDirection,
            ColumnFilters = dto.ColumnFilters ?? new Dictionary<string, string>()
        };

    public static PagedViewerResultDTO ToDto(PagedViewerResult result) =>
        new()
        {
            Columns = result.Columns.Select(ToDto).ToList(),
            Rows = result.Rows,
            TotalRowCount = result.TotalRowCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

    public static ViewerColumnDTO ToDto(ViewerColumn column) =>
        new()
        {
            Key = column.Key,
            Header = column.Header,
            Format = column.Format,
            Sortable = column.Sortable,
            Filterable = column.Filterable
        };
}
