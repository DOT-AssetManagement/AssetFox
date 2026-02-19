using System;
using System.Collections.Generic;

namespace AssetFox.Core.DTOs
{
    public class ViewerMetadataDTO
    {
        public Guid SimulationId { get; set; }
        public string SimulationName { get; set; }
        public int RunId { get; set; }
        public List<int> Years { get; set; } = new List<int>();
        public int AssetCount { get; set; }
        public List<string> AvailableAttributes { get; set; } = new List<string>();
        public bool HasOutput { get; set; }
    }
}
