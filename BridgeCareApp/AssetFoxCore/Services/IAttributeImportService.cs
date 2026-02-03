using System;
using AssetFox.Core.Data.ExcelDatabaseStorage;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services
{
    public interface IAttributeImportService
    {
        AttributesImportResultDTO ImportExcelAttributes(string keyColumnName, string inspectionDateColumnName, string spatialWeightingValue, ExcelRawDataSpreadsheet worksheet);
    }
}
