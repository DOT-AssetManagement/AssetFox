using AssetFox.Core.DTOs;
using AssetFoxCore.Models;
using System.Collections.Generic;
using System;

namespace AssetFoxCore.Interfaces
{
    public interface IPerformanceCurvesPagingService
    {
        PagingPageModel<PerformanceCurveDTO> GetScenarioPage(Guid simulationId, PagingRequestModel<PerformanceCurveDTO> request);
        PagingPageModel<PerformanceCurveDTO> GetLibraryPage(Guid libraryId, PagingRequestModel<PerformanceCurveDTO> request);
        List<PerformanceCurveDTO> GetSyncedScenarioDataSet(Guid simulationId, PagingSyncModel<PerformanceCurveDTO> request);
        List<PerformanceCurveDTO> GetSyncedLibraryDataset(LibraryUpsertPagingRequestModel<PerformanceCurveLibraryDTO, PerformanceCurveDTO> upsertRequest);
    }
}
