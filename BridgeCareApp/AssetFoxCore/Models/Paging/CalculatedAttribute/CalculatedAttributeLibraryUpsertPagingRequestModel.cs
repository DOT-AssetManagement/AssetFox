using AssetFox.Core.DTOs;

namespace AssetFoxCore.Models
{
    public class CalculatedAttributeLibraryUpsertPagingRequestModel : BaseLibraryUpsertPagingRequest<CalculatedAttributeLibraryDTO>
    {
        public CalculatedAttributePagingSyncModel SyncModel { get; set; }
    }
}
