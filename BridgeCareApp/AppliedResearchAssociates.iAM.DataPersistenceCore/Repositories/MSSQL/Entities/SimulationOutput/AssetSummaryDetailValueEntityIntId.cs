using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class AssetSummaryDetailValueEntityIntId: SimulationOutputValueEntityIntId
    {
        public Guid AssetSummaryDetailId { get; set; }

        public virtual AssetSummaryDetailEntity AssetSummaryDetail { get; set; }

    }
}
