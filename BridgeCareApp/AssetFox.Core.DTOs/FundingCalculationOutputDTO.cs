using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class FundingCalculationOutputDTO : BaseDTO
    {
        public IList<AllocationDTO> AllocationMatrix { get; set; } = new List<AllocationDTO>();
    }
}
