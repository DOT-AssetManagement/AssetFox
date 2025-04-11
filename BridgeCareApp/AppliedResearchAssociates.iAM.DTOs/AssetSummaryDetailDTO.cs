using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class AssetSummaryDetailDTO: BaseDTO
    {
        public Guid MaintainableAssetId { get; set; }

        public virtual IList<AssetSummaryDetailValueEntityIntIdDTO> AssetSummaryDetailValuesIntId { get; set; } = new List<AssetSummaryDetailValueEntityIntIdDTO>();
    }
}
