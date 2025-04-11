using System;
using AppliedResearchAssociates.iAM.DTOs;

namespace BridgeCareCore.Services.SimulationCloning
{
    public class FundingCalculationOutputCloner
    {
        internal static FundingCalculationOutputDTO Clone(FundingCalculationOutputDTO fundingCalculationOutput)
        {
            var cloneAllocationMatrix = AllocationCloner.CloneList(fundingCalculationOutput.AllocationMatrix);

            return new FundingCalculationOutputDTO
            {
                Id = Guid.NewGuid(),
                AllocationMatrix = cloneAllocationMatrix
            };
        }
    }
}
