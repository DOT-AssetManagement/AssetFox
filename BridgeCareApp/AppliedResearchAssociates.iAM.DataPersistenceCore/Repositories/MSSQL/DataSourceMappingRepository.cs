using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Extensions;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL
{
    public class DataSourceMappingRepository : IDataSourceMappingRepository
    {
        private readonly UnitOfDataPersistenceWork _unitOfWork;

        public DataSourceMappingRepository(UnitOfDataPersistenceWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<DataSourceMappingDTO> GetDataSourceMappings(Guid dataSourceId)
        {
            _ = _unitOfWork.DataSourceRepo.GetDataSource(dataSourceId) ?? throw new RowNotInTableException($"No datasource found having id {dataSourceId}");

            var dataSourceMappingDtos = _unitOfWork.Context.DataSourceMapping
                .Include(_ => _.Attribute)
                .Where(_ => _.DataSourceId == dataSourceId)
                .Select(_ => _.ToDTO())
                .ToList();                

            return dataSourceMappingDtos;
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
                _unitOfWork.Context.DeleteAll<DataSourceMappingEntity>(_ => _.DataSourceId == dataSourceId);

                // Add mappings
                _unitOfWork.Context.AddAll(dataSourceMappingEntities);
            });
        }
    }
}
