using System;
using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class AssetDetailValueEntityIntIdDTO : BaseDTO
    {
        public Guid AssetDetailId { get; set; }

        public char Discriminator { get; set; }

        public string TextValue { get; set; }

        public double? NumericValue { get; set; }

        public Guid AttributeId { get; set; }
    }
}
