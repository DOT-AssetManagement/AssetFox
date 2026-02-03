using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class SimulationCloningResultDTO : WarningServiceResultDTO
    {
        public SimulationDTO Simulation { get; set; }
    }
}
