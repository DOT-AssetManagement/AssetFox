using AssetFox.Core.DTOs;

namespace AssetFoxCore.Models
{
    public class InvestmentPagingRequestModel : BasePagingRequest
    {
        public InvestmentPagingSyncModel SyncModel { get; set; }
    }
}
