using System;
using AssetFox.Core.Data.ExcelDatabaseStorage.Serializers;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.Data.ExcelDatabaseStorage
{
    public static class ExcelRawDataSpreadsheetSerializationMapper
    {
        public static ExcelRawDataDTO ToDTO(this ExcelRawDataSpreadsheet worksheet, Guid dataSourceID, Guid worksheetId)
        {
            var serializedContent = ExcelRawDataSpreadsheetSerializer.Serialize(worksheet);
            var returnValue = new ExcelRawDataDTO
            {
                Id = worksheetId,
                DataSourceId = dataSourceID,
                SerializedWorksheetContent = serializedContent,
            };
            return returnValue;
        }
    }
}
