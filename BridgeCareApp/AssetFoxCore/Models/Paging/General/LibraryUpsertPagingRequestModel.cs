using System;

namespace AssetFoxCore.Models
{
    public class LibraryUpsertPagingRequestModel<T, Y> : BaseLibraryUpsertPagingRequest<T>
    {
        public LibraryUpsertPagingRequestModel(): base()
        {
            SyncModel = new PagingSyncModel<Y>();
        }
        public PagingSyncModel<Y> SyncModel { get; set; }
    }
}
