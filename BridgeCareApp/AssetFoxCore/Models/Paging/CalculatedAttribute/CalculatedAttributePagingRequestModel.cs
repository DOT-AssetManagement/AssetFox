using System;
using AssetFox.Core.DTOs;
namespace AssetFoxCore.Models
{
    public class CalculatedAttributePagingRequestModel : BasePagingRequest
    {
        public CalculatedAttributePagingRequestModel()
        {
            SyncModel = new CalculatedAttributePagingSyncModel();
        }
        public Guid AttributeId { get; set; }
        public CalculatedAttributePagingSyncModel SyncModel { get; set; }
    }
}
