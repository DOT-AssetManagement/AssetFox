using System;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Interfaces
{
    public interface IDataSourceMappingService
    {
        FileInfoDTO DownloadDataSourceMappings(Guid dataSourceId);
    }
}
