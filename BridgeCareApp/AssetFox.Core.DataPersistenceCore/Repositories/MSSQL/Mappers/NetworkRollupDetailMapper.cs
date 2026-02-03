using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class NetworkRollupDetailMapper
    {
        public static NetworkRollupDetailEntity ToEntity(this NetworkRollupDetailDTO dto) =>
            new NetworkRollupDetailEntity {NetworkId = dto.NetworkId, Status = dto.Status};
    }
}
