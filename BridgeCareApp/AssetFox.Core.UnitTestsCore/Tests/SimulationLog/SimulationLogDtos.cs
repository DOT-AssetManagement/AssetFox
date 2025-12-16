using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class SimulationLogDtos
    {
        public static SimulationLogDTO Dto(Guid simulationId)
        {
            var id = Guid.NewGuid();
            var dto = new SimulationLogDTO
            {
                SimulationId = simulationId,
                Message = "Simulation log",
                Id = id,
                Subject = 123,
            };
            return dto;
        }
    }
}
