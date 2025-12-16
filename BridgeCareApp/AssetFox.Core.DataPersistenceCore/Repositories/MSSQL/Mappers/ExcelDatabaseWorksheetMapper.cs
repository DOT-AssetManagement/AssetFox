using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class ExcelDatabaseWorksheetMapper
    {
        public static ExcelRawDataEntity ToEntity(this ExcelRawDataDTO dto)
        {
            var returnValue = new ExcelRawDataEntity
            {
                Id = dto.Id,
                DataSourceId = dto.DataSourceId,
                SerializedContent = dto.SerializedWorksheetContent,
            };
            return returnValue;
        }

        public static ExcelRawDataDTO ToDTO(this ExcelRawDataEntity entity)
        {
            var returnValue = new ExcelRawDataDTO
            {
                Id = entity.Id,
                SerializedWorksheetContent = entity.SerializedContent,
                DataSourceId = entity.DataSourceId,
            };
            return returnValue;
        }

        internal static ExcelRawDataDTO ToDTONullPropagating(ExcelRawDataEntity entity)
        {
            if (entity == null)
            {
                return null;
            }
            return ToDTO(entity);
        }

        public static DataSourceMappingEntity ToEntity(this DataSourceMappingDTO dto)
        {
            return new DataSourceMappingEntity
            {
                Id = dto.Id,
                AttributeId = dto.AttributeId,
                DataField = dto.DataField,
                DataSourceId = dto.DataSourceId
            };
        }

        public static DataSourceMappingDTO ToDTO(this DataSourceMappingEntity entity)
        {
            return new DataSourceMappingDTO
            {
                Id = entity.Id,
                AttributeId = entity.AttributeId,
                DataField = entity.DataField,
                DataSourceId = entity.DataSourceId,
                AttributeName = entity.Attribute.Name
            };
        }
    }
}
