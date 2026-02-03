using AssetFox.Core.DTOs;
using System;

namespace AssetFoxCore.Services
{
    public interface IExcelRawDataLoadService
    {
        GetRawDataSpreadsheetColumnHeadersResultDTO GetSpreadsheetColumnHeaders(Guid dataSourceId);

    }
}
