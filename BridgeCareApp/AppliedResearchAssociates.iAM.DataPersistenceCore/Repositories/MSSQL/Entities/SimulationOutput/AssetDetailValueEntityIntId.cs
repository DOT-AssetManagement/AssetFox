using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class AssetDetailValueEntityIntId: SimulationOutputValueEntityIntId
    {
        public Guid AssetDetailId { get; set; }

        public int RunId { get; set; }

        public virtual AssetDetailEntity AssetDetail { get; set; }

        public int Id { get; set; }

    }
}
