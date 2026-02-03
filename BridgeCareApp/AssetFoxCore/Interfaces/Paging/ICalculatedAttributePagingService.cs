using AssetFox.Core.DTOs;
using AssetFoxCore.Models;
using System;
using System.Collections.Generic;

namespace AssetFoxCore.Interfaces
{
    public interface ICalculatedAttributePagingService
    {
        PagingPageModel<CalculatedAttributeEquationCriteriaPairDTO> GetScenarioPage(Guid libraryId, CalculatedAttributePagingRequestModel request);
        PagingPageModel<CalculatedAttributeEquationCriteriaPairDTO> GetLibraryPage(Guid simulationId, CalculatedAttributePagingRequestModel request);
        List<CalculatedAttributeDTO> GetSyncedScenarioDataSet(Guid simulationId, CalculatedAttributePagingSyncModel request);
        List<CalculatedAttributeDTO> GetSyncedLibraryDataset(Guid libraryId, CalculatedAttributePagingSyncModel request);
    }
}
