using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs;

namespace BridgeCareCore.Services.SimulationCloning
{
    public class AssetDetailCloner
    {
        internal static IList<AssetDetailDTO> CloneList(IList<AssetDetailDTO> assets)
        {
            var cloneList = new List<AssetDetailDTO>();

            foreach (var asset in assets)
            {
                cloneList.Add(Clone(asset));
            }

            return cloneList;
        }

        internal static AssetDetailDTO Clone(AssetDetailDTO asset)
        {
            var cloneAssetDetailValuesIntId = AssetDetailValueEntityIntIdCloner.CloneList(asset.AssetDetailValuesIntId);
            var cloneTreatmentOptions = TreatmentOptionDetailCloner.CloneList(asset.TreatmentOptions);
            var cloneTreatmentRejections = TreatmentRejectionDetailCloner.CloneList(asset.TreatmentRejections);
            var cloneTreatmentSchedulingCollisions = TreatmentSchedulingCollisionDetailCloner.CloneList(asset.TreatmentSchedulingCollisions);
            var cloneTreatmentConsiderations = TreatmentConsiderationDetailCloner.CloneList(asset.TreatmentConsiderations);

            return new AssetDetailDTO
            {
                Id = Guid.NewGuid(),
                AppliedTreatment = asset.AppliedTreatment,
                MaintainableAssetId = asset.MaintainableAssetId,
                ProjectSource = asset.ProjectSource,
                TreatmentCause = asset.TreatmentCause,
                TreatmentFundingIgnoresSpendingLimit = asset.TreatmentFundingIgnoresSpendingLimit,
                TreatmentStatus = asset.TreatmentStatus,
                AssetDetailValuesIntId = cloneAssetDetailValuesIntId,
                TreatmentOptions = asset.TreatmentOptions,
                TreatmentRejections = asset.TreatmentRejections,
                TreatmentSchedulingCollisions = asset.TreatmentSchedulingCollisions,
                TreatmentConsiderations = asset.TreatmentConsiderations
            };
        }
    }
}
