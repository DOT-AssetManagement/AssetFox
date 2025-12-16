using System;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.DTOs.Abstract;
using OfficeOpenXml;

namespace BridgeCareCore.Services
{
    public interface IExcelRawDataImportService
    {
        WarningServiceResultDTO ImportDataSourceMapping(Guid dataSourceId, ExcelWorksheet worksheet, ExcelWorksheet mappingsWorksheet);

        ExcelRawDataImportResultDTO ImportRawData(Guid dataSourceId, ExcelWorksheet worksheet, bool includeColumnsWithoutTitles = false);
    }
}
