using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IDataSourceMappingRepository
    {
        List<DataSourceMappingDTO> GetDataSourceMappings(Guid dataSourceId);

        void UpsertDataSourceMappings(List<DataSourceMappingDTO> dataSourceMappingDtos, Guid dataSourceId);
    }
}
