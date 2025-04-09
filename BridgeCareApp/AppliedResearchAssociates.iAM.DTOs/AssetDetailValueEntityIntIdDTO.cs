using System;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class AssetDetailValueEntityIntIdDTO
    {
        public int Id { get; set; }

        public Guid AssetDetailId { get; set; }

        public char Discriminator { get; set; }

        public string TextValue { get; set; }

        public double? NumericValue { get; set; }

        public Guid AttributeId { get; set; }
    }
}
