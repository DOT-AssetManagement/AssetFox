using System;
using AppliedResearchAssociates.iAM.DTOs;

namespace BridgeCareCore.Interfaces
{
    public interface IDataSourceMappingService
    {
        FileInfoDTO DownloadDataSourceMappings(Guid dataSourceId);
    }
}
