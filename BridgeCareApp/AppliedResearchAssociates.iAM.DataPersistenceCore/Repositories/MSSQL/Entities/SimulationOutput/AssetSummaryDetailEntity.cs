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

        public virtual MaintainableAssetEntity MaintainableAsset { get; set; } // analysis obj AssetSummaryDetail.AssetName can be traslated to MaintainableAsset.AssetName, Note: it is always null in DB

        public virtual SimulationOutputEntity SimulationOutput { get; set; }

        public Guid SimulationOutputId { get; set; }

        public virtual ICollection<AssetSummaryDetailValueEntityIntId> AssetSummaryDetailValuesIntId { get; set; }

        // TODO check if data populating correctly in AssetSummaryDetailValuesIntId, if so, don't worry abt changing the structure to match to ValuePerNumericAttribute & ValuePerTextAttribute
        // Reports will need re-work to utilize this info

        // ** Below 2 are configured in AssetSummaryDetailValuesIntId

        ///// <summary>
        /////     List the current values of each numeric attribute for the asset.
        ///// </summary>
        //public Dictionary<string, double> ValuePerNumericAttribute { get; } = new();

        ///// <summary>
        /////     List the current values of each text attribute for the asset.
        ///// </summary>
        //public Dictionary<string, string> ValuePerTextAttribute { get; } = new();

    }
}
