using System;
using System.Collections.Generic;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class AssetSummaryDetailEntity
    {
        public AssetSummaryDetailEntity()
        {
            AssetSummaryDetailValuesIntId = new HashSet<AssetSummaryDetailValueEntityIntId>();
        }

        public Guid Id { get; set; }

        public Guid MaintainableAssetId { get; set; }

        public virtual MaintainableAssetEntity MaintainableAsset { get; set; }

        public Guid SimulationOutputId { get; set; }

        public virtual SimulationOutputEntity SimulationOutput { get; set; }        

        public virtual ICollection<AssetSummaryDetailValueEntityIntId> AssetSummaryDetailValuesIntId { get; set; }
    }
}
