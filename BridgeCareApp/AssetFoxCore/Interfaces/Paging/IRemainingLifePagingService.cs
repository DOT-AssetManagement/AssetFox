using AssetFox.Core.DTOs;
using AssetFoxCore.Models;
using System.Collections.Generic;
using System;
using AssetFoxCore.Services.Paging.Generics;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace AssetFoxCore.Interfaces
{
    public interface IRemainingLifeLimitPagingService 
    {
        PagingPageModel<RemainingLifeLimitDTO> GetScenarioPage(Guid scenarioId, PagingRequestModel<RemainingLifeLimitDTO> request);
        PagingPageModel<RemainingLifeLimitDTO> GetLibraryPage(Guid libraryId, PagingRequestModel<RemainingLifeLimitDTO> request);
        List<RemainingLifeLimitDTO> GetSyncedScenarioDataSet(Guid scenarioId, PagingSyncModel<RemainingLifeLimitDTO> syncModel);
        List<RemainingLifeLimitDTO> GetSyncedLibraryDataset(LibraryUpsertPagingRequestModel<RemainingLifeLimitLibraryDTO, RemainingLifeLimitDTO> upsertRequest);
    }
}
