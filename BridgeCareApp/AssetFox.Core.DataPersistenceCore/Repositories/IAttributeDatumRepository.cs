using System;
using System.Collections.Generic;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IAttributeDatumRepository
    {
        void AddAssignedData(List<MaintainableAsset> maintainableAssets, List<AttributeDTO> attributeDtos);

        List<AttributeDatumDTO> GetAllInNetwork(IEnumerable<Guid> networkMaintainableAssetIds, List<Guid> requiredAttributeIds);
    }
}
