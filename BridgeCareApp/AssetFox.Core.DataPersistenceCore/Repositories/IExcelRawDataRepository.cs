using System;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IExcelRawDataRepository
    {
        Guid AddExcelRawData(ExcelRawDataDTO dto);
        ExcelRawDataDTO GetExcelRawDataByDataSourceId(Guid dataSourceId);
    }
}
