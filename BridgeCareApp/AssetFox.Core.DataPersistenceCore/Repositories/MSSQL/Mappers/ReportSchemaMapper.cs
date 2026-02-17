using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class ReportSchemaMapper
    {
        public static ReportSchemaDTO ToDTO(this ReportSchemaEntity entity) =>
            new ReportSchemaDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                SchemaJson = entity.SchemaJson,
                ReportCategory = entity.ReportCategory,
                IsSystemDefault = entity.IsSystemDefault,
                CreatedByName = entity.CreatedBy.ToString(),
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.LastModifiedDate
            };

        public static ReportSchemaDTO ToDTONullPropagating(this ReportSchemaEntity entity)
        {
            if (entity == null)
            {
                return null;
            }
            return ToDTO(entity);
        }

        public static ReportSchemaEntity ToEntity(this ReportSchemaDTO dto)
        {
            var entity = new ReportSchemaEntity
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                SchemaJson = dto.SchemaJson,
                ReportCategory = dto.ReportCategory,
                IsSystemDefault = dto.IsSystemDefault
            };
            return entity;
        }
    }
}
