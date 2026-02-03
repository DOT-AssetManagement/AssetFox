using System;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Abstract;
using OfficeOpenXml;

namespace AssetFoxCore.Services
{
    public interface IExcelRawDataImportService
    {
        WarningServiceResultDTO ImportDataSourceMapping(Guid dataSourceId, ExcelWorksheet worksheet, ExcelWorksheet mappingsWorksheet);

        ExcelRawDataImportResultDTO ImportRawData(Guid dataSourceId, ExcelWorksheet worksheet, bool includeColumnsWithoutTitles = false);
    }
}
