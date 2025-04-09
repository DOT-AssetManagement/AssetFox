using AppliedResearchAssociates.iAM.DTOs;
using System;

namespace BridgeCareCore.Services.SimulationCloning
{
    internal class SimulationOutputCloner
    {
        internal static SimulationOutputDTO Clone(SimulationOutputDTO simulationOutput)
        {
            // Check and add new guid wherever applicable and assign correct new simulationOutput id in all places...
           // var cloneInitialAssetSummaries = AssetSummaryDetailCloner.CloneList(simulationOutput.InitialAssetSummaries);
           // var cloneYears = SimulationYearDetailCloner.CloneList(simulationOutput.Years);
            var clone = new SimulationOutputDTO
            {
                Id = Guid.NewGuid(),
                InitialConditionOfNetwork = simulationOutput.InitialConditionOfNetwork,
             //   InitialAssetSummaries = cloneInitialAssetSummaries,                
               // Years = cloneYears
            };

            return clone;
        }
    }
}
