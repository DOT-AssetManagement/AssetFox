using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class AssetSummaryDetailValueEntityIntId: SimulationOutputValueEntityIntId
    {
        public Guid AssetSummaryDetailId { get; set; }

        public int RunId { get; set; }

        public virtual AssetSummaryDetailEntity AssetSummaryDetail { get; set; }

    }
}
