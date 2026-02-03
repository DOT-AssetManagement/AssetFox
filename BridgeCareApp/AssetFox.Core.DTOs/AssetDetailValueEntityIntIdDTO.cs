using System;

namespace AssetFox.Core.DTOs
{
    public class AssetDetailValueEntityIntIdDTO
    {
        public long Id { get; set; }

        public char Discriminator { get; set; }

        public string TextValue { get; set; }

        public double? NumericValue { get; set; }

        public Guid AttributeId { get; set; }
    }
}
