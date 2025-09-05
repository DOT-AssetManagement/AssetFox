using System;
using System.Collections.Generic;
using System.Linq;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Extensions;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DTOs;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL
{
    public class DataSourceMappingRepository : IDataSourceMappingRepository
    {
        private readonly UnitOfDataPersistenceWork _unitOfWork;

        public DataSourceMappingRepository(UnitOfDataPersistenceWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void UpsertDataSourceMappings(List<DataSourceMappingDTO> dataSourceMappingDtos, Guid dataSourceId)
        {
            _unitOfWork.AsTransaction(() =>
            {                
                var dataSourceMappingEntities = dataSourceMappingDtos.Select(_ =>
                {
                    return _.ToEntity();
                }).ToList();

                // Delete existing mappings
                var temp = _unitOfWork.Context.DataSourceMapping.Select(_ => _.DataSourceId == dataSourceId); // remove later
                _unitOfWork.Context.DeleteAll<DataSourceMappingEntity>(_ => _.DataSourceId == dataSourceId);

                // Add mappings
                _unitOfWork.Context.AddAll(dataSourceMappingEntities);
            });
        }
    }
}
