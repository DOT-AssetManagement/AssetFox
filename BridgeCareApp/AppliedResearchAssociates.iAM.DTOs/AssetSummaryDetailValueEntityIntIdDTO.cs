using System;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class AssetSummaryDetailValueEntityIntIdDTO
    {
        public int Id { get; set; }

        public char Discriminator { get; set; }

        public string TextValue { get; set; }

        public double? NumericValue { get; set; }

        public Guid AttributeId { get; set; }
    }
}
