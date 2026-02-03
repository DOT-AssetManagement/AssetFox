using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class SimulationOutputDTO : BaseDTO
    {
        public double InitialConditionOfNetwork { get; set; }

        public IList<AssetSummaryDetailDTO> InitialAssetSummaries { get; set; } = new List<AssetSummaryDetailDTO>();

        public IList<SimulationYearDetailDTO> Years { get; set; } = new List<SimulationYearDetailDTO>();

        public int RunId { get; set; }
    }
}
