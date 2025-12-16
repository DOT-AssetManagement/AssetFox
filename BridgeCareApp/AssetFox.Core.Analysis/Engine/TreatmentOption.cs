using System;

namespace AppliedResearchAssociates.iAM.Analysis.Engine;

internal sealed class TreatmentOption
{
    public TreatmentOption(AssetContext assetContext, Treatment candidateTreatment, double cost, double benefit, double? remainingLife, double conditionChange)
    {
        AssetContext = assetContext ?? throw new ArgumentNullException(nameof(assetContext));
        CandidateTreatment = candidateTreatment ?? throw new ArgumentNullException(nameof(candidateTreatment));
        Cost = cost;
        Benefit = benefit;
        RemainingLife = remainingLife;
        ConditionChange = conditionChange;

        var unweightedObjectiveValue = assetContext.SimulationRunner.ObjectiveFunction(this);
        var spatialWeight = assetContext.Detail.SpatialWeightForOrderingOptions ?? double.NaN;

        WeightedObjectiveValue = unweightedObjectiveValue * spatialWeight;
    }

    public double Benefit { get; }

    public Treatment CandidateTreatment { get; }

    public double ConditionChange { get; }

    public AssetContext AssetContext { get; }

    public double Cost { get; }

    public TreatmentOptionDetail Detail => new(CandidateTreatment.Name, Cost, Benefit, RemainingLife, ConditionChange);

    public double? RemainingLife { get; }

    public double WeightedObjectiveValue { get; }
}
