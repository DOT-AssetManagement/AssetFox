using AssetFox.Core.DTOs;
using AssetFoxCore.Models;
using System.Collections.Generic;
using System;

namespace AssetFoxCore.Interfaces
{
    public interface ICashFlowPagingService
    {
        PagingPageModel<CashFlowRuleDTO> GetScenarioPage(Guid simulationId, PagingRequestModel<CashFlowRuleDTO> request);
        PagingPageModel<CashFlowRuleDTO> GetLibraryPage(Guid libraryId, PagingRequestModel<CashFlowRuleDTO> request);
        List<CashFlowRuleDTO> GetSyncedScenarioDataSet(Guid simulationId, PagingSyncModel<CashFlowRuleDTO> request);
        List<CashFlowRuleDTO> GetSyncedLibraryDataset(LibraryUpsertPagingRequestModel<CashFlowRuleLibraryDTO, CashFlowRuleDTO> request);
    }
}
