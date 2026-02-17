using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IReportSchemaRepository
    {
        void Upsert(ReportSchemaDTO dto);
        ReportSchemaDTO Get(Guid id);
        List<ReportSchemaDTO> GetAll();
        List<ReportSchemaDTO> GetByCategory(string category);
        List<ReportSchemaDTO> GetDefaults();
        void Delete(Guid id);
    }
}
