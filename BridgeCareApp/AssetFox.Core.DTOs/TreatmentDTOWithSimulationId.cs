using System;
using System.Collections.Generic;
using System.Text;

namespace AssetFox.Core.DTOs
{
    public class TreatmentDTOWithSimulationId
    {
        public TreatmentDTO Treatment { get; set; }
        public Guid SimulationId { get; set; }
    }
}
