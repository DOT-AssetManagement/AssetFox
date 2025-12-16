using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class RemainingLifeLimitLibraryDTO : BaseLibraryDTO
    {
        public List<RemainingLifeLimitDTO> RemainingLifeLimits { get; set; } = new List<RemainingLifeLimitDTO>();
    }
}
