using System;
using System.Collections.Generic;

namespace AssetFox.Core.DTOs
{
    public class MaintainableAssetQueryDTO
    {
        public Guid AssetId { get; set; }
        public Dictionary<AttributeDTO, string> AssetProperties { get; set; }
    }
}
