using AssetFox.Core.DTOs;
using AssetFoxCore.Models;
using System.Collections.Generic;
using System;

namespace AssetFoxCore.Interfaces
{
    public interface IInvestmentPagingService
    {
        InvestmentPagingPageModel GetLibraryPage(Guid libraryId, InvestmentPagingRequestModel request);

        InvestmentPagingPageModel GetScenarioPage(Guid simulationId, InvestmentPagingRequestModel request);

        List<BudgetDTO> GetSyncedScenarioDataSet(Guid simulationId, InvestmentPagingSyncModel request);

        List<BudgetDTO> GetSyncedLibraryDataset(Guid libraryId, InvestmentPagingSyncModel request);
        List<BudgetDTO> GetNewLibraryDataset(InvestmentPagingSyncModel request);
    }
}
