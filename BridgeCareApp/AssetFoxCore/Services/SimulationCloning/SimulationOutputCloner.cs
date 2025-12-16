using AssetFox.Core.DTOs;
using System;

namespace AssetFoxCore.Services.SimulationCloning
{
    internal class SimulationOutputCloner
    {
        internal static SimulationOutputDTO Clone(SimulationOutputDTO simulationOutput)
        {
            var cloneInitialAssetSummaries = AssetSummaryDetailCloner.CloneList(simulationOutput.InitialAssetSummaries);
            var cloneYears = SimulationYearDetailCloner.CloneList(simulationOutput.Years);

            var clone = new SimulationOutputDTO
            {
                Id = Guid.NewGuid(),
                InitialConditionOfNetwork = simulationOutput.InitialConditionOfNetwork,
                InitialAssetSummaries = cloneInitialAssetSummaries,
                Years = cloneYears
            };

            return clone;
        }
    }
}
