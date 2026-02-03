namespace AssetFox.Core.Analysis.Engine;

public enum ReasonForCancellationOfFunding
{
    None,
    CouldNotSelectTreatmentsForAllOpenAssetsInGroup,
    CouldNotSelectSameTreatmentForAllOpenAssetsInGroup,
}
