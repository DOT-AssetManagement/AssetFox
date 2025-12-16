using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories
{
    public interface IDataSourceMappingRepository
    {
        List<DataSourceMappingDTO> GetDataSourceMappings(Guid dataSourceId);

        void UpsertDataSourceMappings(List<DataSourceMappingDTO> dataSourceMappingDtos, Guid dataSourceId);
    }
}
