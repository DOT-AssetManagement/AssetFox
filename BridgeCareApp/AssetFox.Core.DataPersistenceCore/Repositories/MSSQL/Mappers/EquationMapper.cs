using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class EquationMapper
    {
        public static EquationEntity ToEntity(this Equation domain) =>
            new EquationEntity { Id = domain.Id, Expression = domain.Expression };

        public static EquationEntity ToEntity(this EquationDTO dto, BaseEntityProperties baseEntityProperties = null)
        {
            var entity = new EquationEntity
            {
                Id = dto.Id,
                Expression = dto.Expression
            };
            BaseEntityPropertySetter.SetBaseEntityProperties(entity, baseEntityProperties);
            return entity;
        }

            public static EquationDTO ToDto(this EquationEntity entity) =>
                new EquationDTO { Id = entity.Id, Expression = entity.Expression };
        }
    }
