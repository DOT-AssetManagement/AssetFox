using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class FundingCalculationOutputDTO : BaseDTO
    {
        public Guid TreatmentConsiderationDetailId { get; set; }

        public IList<AllocationDTO> AllocationMatrix { get; set; } = new List<AllocationDTO>();
    }
}
