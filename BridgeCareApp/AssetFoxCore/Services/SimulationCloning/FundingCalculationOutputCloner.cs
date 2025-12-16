using System;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.SimulationCloning
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
