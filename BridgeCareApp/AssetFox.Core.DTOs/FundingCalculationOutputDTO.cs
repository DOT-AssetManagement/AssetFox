using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class FundingCalculationOutputDTO : BaseDTO
    {
        public IList<AllocationDTO> AllocationMatrix { get; set; } = new List<AllocationDTO>();
    }
}
