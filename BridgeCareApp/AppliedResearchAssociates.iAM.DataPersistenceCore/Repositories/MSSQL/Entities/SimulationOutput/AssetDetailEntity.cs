using System;
using System.Collections.Generic;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class AssetDetailEntity
    {
        public Guid Id { get; set; }

        public Guid MaintainableAssetId { get; set; }

        public virtual MaintainableAssetEntity MaintainableAsset { get; set; }

        public Guid SimulationYearDetailId { get; set; }

        public virtual SimulationYearDetailEntity SimulationYearDetail { get; set; }

        public string AppliedTreatment { get; set; }

        public int TreatmentCause { get; set; }

        public ICollection<AssetDetailValueEntityIntId> AssetDetailValuesIntId { get; set; } = new HashSet<AssetDetailValueEntityIntId>();

        public ICollection<TreatmentConsiderationDetailEntity> TreatmentConsiderations { get; set; } = new HashSet<TreatmentConsiderationDetailEntity>();

        public bool TreatmentFundingIgnoresSpendingLimit { get; set; }

        public ICollection<TreatmentOptionDetailEntity> TreatmentOptions { get; set; } = new HashSet<TreatmentOptionDetailEntity>();

        public ICollection<TreatmentRejectionDetailEntity> TreatmentRejections { get; set; } = new HashSet<TreatmentRejectionDetailEntity>();

        public ICollection<TreatmentSchedulingCollisionDetailEntity> TreatmentSchedulingCollisions { get; set; } = new HashSet<TreatmentSchedulingCollisionDetailEntity>();

        public int TreatmentStatus { get; set; }

        public string ProjectSource { get; set; }

    }
}
