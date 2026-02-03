using AssetFox.Core.DTOs;
using AssetFoxCore.Models;
using System.Collections.Generic;
using System;
using AssetFoxCore.Services.Paging.Generics;

namespace AssetFoxCore.Interfaces
{
    public interface IBudgetPriortyPagingService 
    {
        PagingPageModel<BudgetPriorityDTO> GetScenarioPage(Guid simulationId, PagingRequestModel<BudgetPriorityDTO> request);
        PagingPageModel<BudgetPriorityDTO> GetLibraryPage(Guid libraryId, PagingRequestModel<BudgetPriorityDTO> request);
        List<BudgetPriorityDTO> GetSyncedScenarioDataSet(Guid simulationId, PagingSyncModel<BudgetPriorityDTO> request);
        List<BudgetPriorityDTO> GetSyncedLibraryDataset(LibraryUpsertPagingRequestModel<BudgetPriorityLibraryDTO, BudgetPriorityDTO> request);
    }
}
