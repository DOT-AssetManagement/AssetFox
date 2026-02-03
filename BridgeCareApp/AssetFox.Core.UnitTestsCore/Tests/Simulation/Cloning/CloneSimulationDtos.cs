using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests.SimulationCloning
{
    public static class CloneSimulationDtos
    {
        public static CloneSimulationDTO Create(Guid sourceSimulationId, Guid networkId, string newSimulationName)
        {
            var dto = new CloneSimulationDTO
            {
                Id = Guid.NewGuid(),
                NetworkId = networkId,
                ScenarioId = sourceSimulationId,
                ScenarioName = newSimulationName,
            };
            return dto;
        }
    }
}
