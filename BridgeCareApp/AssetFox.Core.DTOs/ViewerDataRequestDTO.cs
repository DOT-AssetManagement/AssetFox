using System;
using System.Collections.Generic;

namespace AssetFox.Core.DTOs
{
    public class ViewerDataRequestDTO
    {
        public Guid SimulationId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string SortBy { get; set; }
        public string SortDirection { get; set; } = "asc";
        public Dictionary<string, string> ColumnFilters { get; set; } = new Dictionary<string, string>();
    }
}
