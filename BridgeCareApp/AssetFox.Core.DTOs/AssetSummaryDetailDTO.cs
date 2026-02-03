using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class AssetSummaryDetailDTO: BaseDTO
    {
        public Guid MaintainableAssetId { get; set; }

        public virtual IList<AssetSummaryDetailValueEntityIntIdDTO> AssetSummaryDetailValuesIntId { get; set; } = new List<AssetSummaryDetailValueEntityIntIdDTO>();
    }
}
