using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IReportSchemaRepository
    {
        void Upsert(ReportSchemaDTO dto, Guid? ownerUserId = null);
        ReportSchemaDTO Get(Guid id);
        ReportSchemaDTO GetAccessible(Guid id, Guid userId, bool includeAll);
        List<ReportSchemaDTO> GetAll();
        List<ReportSchemaDTO> GetAccessibleSummaries(Guid userId, bool includeAll);
        List<ReportSchemaDTO> GetByCategory(string category);
        List<ReportSchemaDTO> GetDefaults();
        LibraryUserAccessModel GetLibraryAccess(Guid reportSchemaId, Guid userId);
        List<LibraryUserDTO> GetLibraryUsers(Guid reportSchemaId);
        void UpsertOrDeleteUsers(Guid reportSchemaId, IList<LibraryUserDTO> users);
        void Delete(Guid id);
    }
}
