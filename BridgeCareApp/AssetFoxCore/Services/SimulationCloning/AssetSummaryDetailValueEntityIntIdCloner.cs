using AssetFox.Core.DTOs;
using System.Collections.Generic;

namespace AssetFoxCore.Services.SimulationCloning
{
    public class AssetSummaryDetailValueEntityIntIdCloner
    {
        internal static IList<AssetSummaryDetailValueEntityIntIdDTO> CloneList(IList<AssetSummaryDetailValueEntityIntIdDTO> AssetSummaryDetailValuesIntId)
        {
            var cloneList = new List<AssetSummaryDetailValueEntityIntIdDTO>();

            foreach (var AssetSummaryDetailValueIntId in AssetSummaryDetailValuesIntId)
            {
                cloneList.Add(Clone(AssetSummaryDetailValueIntId));
            }

            return cloneList;
        }

        internal static AssetSummaryDetailValueEntityIntIdDTO Clone(AssetSummaryDetailValueEntityIntIdDTO assetSummaryDetailValueEntityIntId)
        {
            return new AssetSummaryDetailValueEntityIntIdDTO
            {
                AttributeId = assetSummaryDetailValueEntityIntId.AttributeId,
                Discriminator = assetSummaryDetailValueEntityIntId.Discriminator,
                NumericValue = assetSummaryDetailValueEntityIntId.NumericValue,
                TextValue = assetSummaryDetailValueEntityIntId.TextValue
            };
        }
    }
}
