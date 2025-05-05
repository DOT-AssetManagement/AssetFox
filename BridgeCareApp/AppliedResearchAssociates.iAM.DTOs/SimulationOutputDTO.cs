using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class SimulationOutputDTO : BaseDTO
    {
        public double InitialConditionOfNetwork { get; set; }

        public IList<AssetSummaryDetailDTO> InitialAssetSummaries { get; set; } = new List<AssetSummaryDetailDTO>();

        public IList<SimulationYearDetailDTO> Years { get; set; } = new List<SimulationYearDetailDTO>();

        public int RunId { get; set; }
    }
}
