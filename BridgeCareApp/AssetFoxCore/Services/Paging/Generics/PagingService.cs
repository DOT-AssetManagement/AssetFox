using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using AssetFox.Core.DTOs.Abstract;
using AssetFoxCore.Models;

namespace AssetFoxCore.Services.Paging.Generics
{
    public abstract class PagingService<T, Y> : BasePagingService<T>
        where T : BaseDTO
        where Y : BaseLibraryDTO 
    {
        public virtual PagingPageModel<T> GetScenarioPage(Guid scenarioId, PagingRequestModel<T> request)
        {
            var rows = request.SyncModel.LibraryId == null ? GetScenarioRows(scenarioId) :
                GetLibraryRows(request.SyncModel.LibraryId.Value);

            return HandlePaging(rows, request);
        }

        public virtual PagingPageModel<T> GetLibraryPage(Guid libraryId, PagingRequestModel<T> request)
        {
            var rows = GetLibraryRows(libraryId);

            return HandlePaging(rows, request);
        }

        public virtual List<T> GetSyncedScenarioDataSet(Guid scenarioId, PagingSyncModel<T> syncModel)
        {
            var rows = syncModel.LibraryId == null ?
                    GetScenarioRows(scenarioId) :
                    GetLibraryRows(syncModel.LibraryId.Value);
            rows = SyncDataset(rows, syncModel);
            if (syncModel.LibraryId != null)
                rows = CreateAsNewDataset(rows);
            return rows;
        }

        public virtual List<T> GetSyncedLibraryDataset(LibraryUpsertPagingRequestModel<Y, T> upsertRequest)
        {
            Guid? libraryId = null;

            var rows = new List<T>();
            if (upsertRequest.ScenarioId != null)
                rows = GetSyncedScenarioDataSet(upsertRequest.ScenarioId.Value, upsertRequest.SyncModel);
            else
            {
                if (upsertRequest.SyncModel.LibraryId != null)
                    libraryId = upsertRequest.SyncModel.LibraryId.Value;
                else if (!upsertRequest.IsNewLibrary)
                    libraryId = upsertRequest.Library.Id;

                if (libraryId != null)
                    rows = GetLibraryRows(libraryId.Value);
                rows = SyncDataset(rows, upsertRequest.SyncModel);
            }

            // GetSyncedScenarioDataSet already gives rows fresh IDs when a library is selected
            // as the scenario source. Cloning those rows again loses the original source IDs
            // needed to remap relationships such as treatment rule override targets.
            var scenarioRowsAlreadyHaveFreshIds = upsertRequest.ScenarioId != null
                && upsertRequest.SyncModel.LibraryId != null;
            if (upsertRequest.IsNewLibrary && !scenarioRowsAlreadyHaveFreshIds)
            {
                rows = CreateAsNewDataset(rows);
            }

            return rows;
        }

        protected virtual List<T> GetScenarioRows(Guid scenarioId) => throw new NotImplementedException();
        protected virtual List<T> GetLibraryRows(Guid libraryId) => throw new NotImplementedException();
        protected virtual List<T> CreateAsNewDataset(List<T> rows) => throw new NotImplementedException();
    }
}
