using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.Data.Networking;
using AppliedResearchAssociates.iAM.Analysis;
using AppliedResearchAssociates.iAM.Data;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories
{
    public interface IMaintainableAssetRepository
    {
        List<Data.Networking.MaintainableAsset> GetAllInNetworkWithAssignedDataAndLocations(Guid networkId);
        List<Data.Networking.MaintainableAsset> GetAllInNetworkWithLocations(Guid networkId);
        bool CheckIfKeyAttributeValueExists(Guid networkId, string attributeValue);
        Dictionary<string, bool> CheckIfKeyAttributeValuesExists(Guid networkId, List<string> attributeValues);

        void CreateMaintainableAssets(List<MaintainableAsset> maintainableAssets, Guid networkId);

        void UpdateMaintainableAssetsSpatialWeighting(List<Data.Networking.MaintainableAsset> maintainableAssets);

        string GetPredominantAssetSpatialWeighting(Guid networkId);

        MaintainableAsset GetMaintainableAssetByKeyAttribute(Guid networkId, string attributeValue);

        List<Guid> GetAllIdsInCommittedProjectsForSimulation(Guid simulationId, Guid networkId);

        List<Guid> GetMaintainableAssetAttributeIdsByNetworkId(Guid networkId);

    }
}
