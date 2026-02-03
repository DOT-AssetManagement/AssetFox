using System;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services
{
    internal class AnalysisMethodCloner
    {
        internal static AnalysisMethodDTO Clone(AnalysisMethodDTO analysisMethod, Guid ownerId, bool isToSameNetwork)
        {
            var cloneBenefit = BenefitCloner.Clone(analysisMethod.Benefit);
            var cloneCriterionLibrary = CriterionLibraryCloner.CloneNullPropagating(analysisMethod.CriterionLibrary, ownerId);
            var clone = new AnalysisMethodDTO
            {
                Id = Guid.NewGuid(),
                Attribute = analysisMethod.Attribute,
                Description = analysisMethod.Description,
                Benefit = cloneBenefit,
                CriterionLibrary = cloneCriterionLibrary,
                OptimizationStrategy = analysisMethod.OptimizationStrategy,
                ShouldApplyMultipleFeasibleCosts = analysisMethod.ShouldApplyMultipleFeasibleCosts,
                ShouldDeteriorateDuringCashFlow = analysisMethod.ShouldDeteriorateDuringCashFlow,
                ShouldUseExtraFundsAcrossBudgets = analysisMethod.ShouldUseExtraFundsAcrossBudgets,
                ShouldAllowMultipleTreatments = analysisMethod.ShouldAllowMultipleTreatments,
                SpendingStrategy = analysisMethod.SpendingStrategy,
                LastKnownAssetCount = isToSameNetwork ? analysisMethod.LastKnownAssetCount : -1,
            };
            return clone;
        }
            
    }
}
