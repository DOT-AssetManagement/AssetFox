using AppliedResearchAssociates.iAM.DTOs;
using System;

namespace BridgeCareCore.Services.SimulationCloning
{
    internal class SimulationOutputCloner
    {
        internal static SimulationOutputDTO Clone(SimulationOutputDTO simulationOutput)
        {
            var clone = new SimulationOutputDTO
            {
                Id = Guid.NewGuid(),
                InitialAssetSummaries = simulationOutput.InitialAssetSummaries,
                InitialConditionOfNetwork = simulationOutput.InitialConditionOfNetwork,
                Years = simulationOutput.Years
            };

            return clone;
        }
    }
}
