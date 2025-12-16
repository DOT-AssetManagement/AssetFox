using AssetFox.Core.DataMiner.Attributes;

namespace AssetFox.Core.DataPersistenceCore.Models
{
    public class Segment
    {
        public Segment(Location location, IAttributeDatum segmentationAttributeDatum)
        {
            Location = location;
            SegmentationAttributeDatum = segmentationAttributeDatum;
        }

        public Location Location { get; }
        public IAttributeDatum SegmentationAttributeDatum { get; }
    }
}
