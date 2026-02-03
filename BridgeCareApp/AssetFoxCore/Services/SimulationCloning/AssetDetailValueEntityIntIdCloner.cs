using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.SimulationCloning
{
    public class AssetDetailValueEntityIntIdCloner
    {
        internal static IList<AssetDetailValueEntityIntIdDTO> CloneList(IList<AssetDetailValueEntityIntIdDTO> assetDetailValuesIntId)
        {
            var cloneList = new List<AssetDetailValueEntityIntIdDTO>();

            foreach (var assetDetailValueIntId in assetDetailValuesIntId)
            {
                cloneList.Add(Clone(assetDetailValueIntId));
            }

            return cloneList;
        }

        internal static AssetDetailValueEntityIntIdDTO Clone(AssetDetailValueEntityIntIdDTO assetDetailValueEntityIntId)
        {
            return new AssetDetailValueEntityIntIdDTO
            {
                AttributeId = assetDetailValueEntityIntId.AttributeId,
                Discriminator = assetDetailValueEntityIntId.Discriminator,
                NumericValue = assetDetailValueEntityIntId.NumericValue,
                TextValue = assetDetailValueEntityIntId.TextValue
            };
        }
    }
}
