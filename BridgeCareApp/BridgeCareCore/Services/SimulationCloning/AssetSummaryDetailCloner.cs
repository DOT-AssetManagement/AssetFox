using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs;

namespace BridgeCareCore.Services.SimulationCloning
{
    public class AssetSummaryDetailCloner
    {
        internal static IList<AssetSummaryDetailDTO> CloneList(IList<AssetSummaryDetailDTO> initialAssetSummaries)
        {
            var cloneList = new List<AssetSummaryDetailDTO>();

            foreach (var initialAssetSummary in initialAssetSummaries)
            {                
                cloneList.Add(Clone(initialAssetSummary));
            }

            return cloneList;
        }

        internal static AssetSummaryDetailDTO Clone(AssetSummaryDetailDTO initialAssetSummary)
        {
            var cloneAssetSummaryDetailValuesIntId = AssetSummaryDetailValueEntityIntIdCloner.CloneList(initialAssetSummary.AssetSummaryDetailValuesIntId);

            return new AssetSummaryDetailDTO
            {
                Id = Guid.NewGuid(),
                MaintainableAssetId = initialAssetSummary.MaintainableAssetId,                
                AssetSummaryDetailValuesIntId = cloneAssetSummaryDetailValuesIntId
            };
        }
    }
}
