using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Models
{
    public class InvestmentLibraryUpsertPagingRequestModel : BaseLibraryUpsertPagingRequest<BudgetLibraryDTO>
    {
        public InvestmentLibraryUpsertPagingRequestModel()
        {
            SyncModel = new InvestmentPagingSyncModel();
        }
        public InvestmentPagingSyncModel SyncModel { get; set; }
    }
}
