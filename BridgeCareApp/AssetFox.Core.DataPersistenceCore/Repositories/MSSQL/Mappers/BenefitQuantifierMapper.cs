using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class BenefitQuantifierMapper
    {
        public static BenefitQuantifierDTO ToDto(this BenefitQuantifierEntity entity) =>
            new BenefitQuantifierDTO
            {
                NetworkId = entity.NetworkId,
                Equation = entity.Equation?.ToDto() ?? new EquationDTO {Id = Guid.NewGuid()}
            };

        public static BenefitQuantifierEntity ToEntity(this BenefitQuantifierDTO dto) =>
            new BenefitQuantifierEntity {NetworkId = dto.NetworkId, EquationId = dto.Equation.Id};
    }
}
