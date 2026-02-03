using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class SimulationReportDetailDtos
    {
        public static SimulationReportDetailDTO Dto(Guid simulationId)
        {
            var dto = new SimulationReportDetailDTO
            {
                SimulationId = simulationId,
                Status = "Simulation report status",
                ReportType = "Report type",
            };
            return dto;
        }
    }
}
