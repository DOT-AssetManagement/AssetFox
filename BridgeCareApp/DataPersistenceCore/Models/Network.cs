using System.Collections.Generic;

namespace AssetFox.Core.DataPersistenceCore.Models
{
    public class Network
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Segment> Segments { get; set; }
    }
}
