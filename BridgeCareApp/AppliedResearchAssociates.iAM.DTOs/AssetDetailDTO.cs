using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs.Abstract;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class AssetDetailDTO : BaseDTO
    {
        public Guid MaintainableAssetId { get; set; }

        public string AppliedTreatment { get; set; }

        public int TreatmentCause { get; set; }

        public bool TreatmentFundingIgnoresSpendingLimit { get; set; }

        public int TreatmentStatus { get; set; }

        public string ProjectSource { get; set; }

        public IList<AssetDetailValueEntityIntIdDTO> AssetDetailValuesIntId { get; set; } = new List<AssetDetailValueEntityIntIdDTO>();

        public IList<TreatmentConsiderationDetailDTO> TreatmentConsiderations { get; set; } = new List<TreatmentConsiderationDetailDTO>();        

        public IList<TreatmentOptionDetailDTO> TreatmentOptions { get; set; } = new List<TreatmentOptionDetailDTO>();

        public IList<TreatmentRejectionDetailDTO> TreatmentRejections { get; set; } = new List<TreatmentRejectionDetailDTO>();

        public IList<TreatmentSchedulingCollisionDetailDTO> TreatmentSchedulingCollisions { get; set; } = new List<TreatmentSchedulingCollisionDetailDTO>();
    }
}
