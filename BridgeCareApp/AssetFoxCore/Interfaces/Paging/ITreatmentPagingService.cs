using AssetFox.Core.DTOs;
using AssetFoxCore.Models;
using System.Collections.Generic;
using System;

namespace AssetFoxCore.Interfaces
{
    public interface ITreatmentPagingService
    {
        List<TreatmentDTO> GetSyncedLibraryDataset(LibraryUpsertPagingRequestModel<TreatmentLibraryDTO, TreatmentDTO> upsertRequest);

        List<TreatmentDTO> GetSyncedScenarioDataSet(Guid simulationId, PagingSyncModel<TreatmentDTO> request);
    }
}

