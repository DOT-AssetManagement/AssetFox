using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class SimulationUserMapper
    {
        public static SimulationUserEntity ToEntity(this SimulationUserDTO dto, Guid simulationId, BaseEntityProperties baseEntityProperties = null)
        {
            var entity = new SimulationUserEntity
            {
                SimulationId = simulationId,
                UserId = dto.UserId,
                CanModify = dto.CanModify,
                IsOwner = dto.IsOwner
            };
            BaseEntityPropertySetter.SetBaseEntityProperties(entity, baseEntityProperties);
            return entity;
        }

        public static SimulationUserDTO ToDto(this SimulationUserEntity entity) =>
            new SimulationUserDTO
            {
                UserId = entity.User.Id,
                CanModify = entity.CanModify,
                IsOwner = entity.IsOwner,
                Username = entity.User.Username
            };
    }
}
