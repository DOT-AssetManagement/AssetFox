using System;
using AssetFox.Core.DTOs.Enums;

namespace AssetFoxCore.Models.DefaultData
{
    public class AnalysisDefaultData
    {
        public string Weighting { get; set; }

        public OptimizationStrategy OptimizationStrategy { get; set; }

        public string BenefitAttribute { get; set; }

        public int BenefitLimit { get; set; }

        public SpendingStrategy SpendingStrategy { get; set; }

        public bool ShouldUseSuperAssets { get; set; }

        public Guid? SuperAssetAttribute { get; set; }
    }
}
