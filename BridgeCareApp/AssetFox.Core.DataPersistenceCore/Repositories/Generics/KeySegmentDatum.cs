using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.Generics
{
    public class KeySegmentDatum
    {
        public Guid AssetId { get; set; }
        public SegmentAttributeDatum KeyValue { get; set; }
    }
}
