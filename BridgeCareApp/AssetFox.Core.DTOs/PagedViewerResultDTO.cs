using System;
using System.Collections.Generic;

namespace AssetFox.Core.DTOs
{
    public class PagedViewerResultDTO
    {
        public List<ViewerColumnDTO> Columns { get; set; } = new List<ViewerColumnDTO>();
        public List<List<object>> Rows { get; set; } = new List<List<object>>();
        public int TotalRowCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalRowCount / PageSize) : 0;
    }

    public class ViewerColumnDTO
    {
        public string Key { get; set; }
        public string Header { get; set; }
        public string Format { get; set; } = "text";
        public bool Sortable { get; set; } = true;
        public bool Filterable { get; set; } = true;
    }
}
