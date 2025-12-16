using System;
using System.ComponentModel.DataAnnotations;

namespace AssetFox.Core.DTOs
{
    public class SimulationReportDetailDTO
    {
        public Guid SimulationId { get; set; }

        public string Status { get; set; }

        public string ReportType { get; set; }
    }
}
