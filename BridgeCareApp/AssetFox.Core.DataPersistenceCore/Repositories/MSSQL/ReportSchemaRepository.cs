using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Extensions;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL
{
    public class ReportSchemaRepository : IReportSchemaRepository
    {
        private readonly UnitOfDataPersistenceWork _unitOfDataPersistenceWork;

        public ReportSchemaRepository(UnitOfDataPersistenceWork unitOfDataPersistenceWork)
        {
            _unitOfDataPersistenceWork = unitOfDataPersistenceWork ?? throw new ArgumentNullException(nameof(unitOfDataPersistenceWork));
        }

        public void Upsert(ReportSchemaDTO dto)
        {
            var entity = dto.ToEntity();
            var existingEntity = _unitOfDataPersistenceWork.Context.ReportSchema
                .FirstOrDefault(_ => _.Id == dto.Id);

            if (existingEntity != null)
            {
                existingEntity.Name = entity.Name;
                existingEntity.Description = entity.Description;
                existingEntity.SchemaJson = entity.SchemaJson;
                existingEntity.ReportCategory = entity.ReportCategory;
                existingEntity.IsSystemDefault = entity.IsSystemDefault;
                existingEntity.LastModifiedDate = DateTime.Now;
                _unitOfDataPersistenceWork.Context.SaveChanges();
            }
            else
            {
                _unitOfDataPersistenceWork.Context.AddEntity(entity, _unitOfDataPersistenceWork.UserEntity?.Id);
            }
        }

        public ReportSchemaDTO Get(Guid id)
        {
            return _unitOfDataPersistenceWork.Context.ReportSchema
                .FirstOrDefault(_ => _.Id == id)
                .ToDTONullPropagating();
        }

        public List<ReportSchemaDTO> GetAll()
        {
            return _unitOfDataPersistenceWork.Context.ReportSchema
                .Select(_ => _.ToDTO())
                .ToList();
        }

        public List<ReportSchemaDTO> GetByCategory(string category)
        {
            return _unitOfDataPersistenceWork.Context.ReportSchema
                .Where(_ => _.ReportCategory == category)
                .Select(_ => _.ToDTO())
                .ToList();
        }

        public List<ReportSchemaDTO> GetDefaults()
        {
            return _unitOfDataPersistenceWork.Context.ReportSchema
                .Where(_ => _.IsSystemDefault)
                .Select(_ => _.ToDTO())
                .ToList();
        }

        public void Delete(Guid id)
        {
            _unitOfDataPersistenceWork.Context.DeleteEntity<ReportSchemaEntity>(_ => _.Id == id);
        }
    }
}
