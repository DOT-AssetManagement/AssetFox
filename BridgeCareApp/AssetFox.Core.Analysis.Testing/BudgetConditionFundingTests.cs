using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.Analysis.Testing.CharacterizationTesting;
using AssetFox.Core.DTOs.Enums;
using MaintainableAsset = AssetFox.Core.Analysis.Input.DataTransfer.MaintainableAsset;
using TextAttributeHistory = AssetFox.Core.Analysis.Input.DataTransfer.AttributeValueHistory<string>;

namespace AssetFox.Core.Analysis.Testing;

public class BudgetConditionFundingTests
{
    [Theory]
    [InlineData(SpendingStrategy.UnlimitedSpending)]
    [InlineData(SpendingStrategy.AsBudgetPermits)]
    public void AssetExcludedByEveryBudgetConditionIsNotTreated(SpendingStrategy spendingStrategy)
    {
        var scenario = InputCreation.CreateExtremelyMinimalInput();
        scenario.AnalysisMethod.SpendingStrategy = spendingStrategy;

        scenario.Network.AttributeSystem.TextAttributes.Add(new()
        {
            Name = "DISTRICT",
            DefaultValue = "0",
        });

        var includedAsset = scenario.Network.MaintainableAssets.Single();
        includedAsset.ID = Guid.NewGuid();
        includedAsset.TextAttributeHistories.Add(DistrictHistory("1"));

        var excludedAsset = new MaintainableAsset
        {
            ID = Guid.NewGuid(),
            Name = "LA 2",
            NumberAttributeHistories = includedAsset.NumberAttributeHistories,
            SpatialWeightExpression = includedAsset.SpatialWeightExpression,
            TextAttributeHistories = { DistrictHistory("2") },
        };

        scenario.Network.MaintainableAssets.Add(excludedAsset);

        scenario.InvestmentPlan.BudgetConditions.Add(new()
        {
            BudgetName = scenario.InvestmentPlan.Budgets.Single().Name,
            ConditionExpression = "[DISTRICT]='1'",
        });

        var simulation = scenario.ConvertOut();
        new SimulationRunner(simulation, true).Run();

        var assetDetails = simulation.Results.Years.Single().Assets;

        var includedDetail = assetDetails.Single(detail => detail.AssetName == includedAsset.Name);
        Assert.Equal(TreatmentCause.SelectedTreatment, includedDetail.TreatmentCause);
        Assert.True(AllocatedAmount(includedDetail) > 0);

        var excludedDetail = assetDetails.Single(detail => detail.AssetName == excludedAsset.Name);
        Assert.Equal(TreatmentCause.NoSelection, excludedDetail.TreatmentCause);
        Assert.Equal(scenario.NameOfPassiveTreatment, excludedDetail.AppliedTreatment);
        Assert.Equal(0, AllocatedAmount(excludedDetail));
    }

    private static TextAttributeHistory DistrictHistory(string district) => new()
    {
        AttributeName = "DISTRICT",
        History =
        {
            new()
            {
                Value = district,
                Year = 2015,
            },
        },
    };

    private static decimal AllocatedAmount(AssetDetail detail) => detail.TreatmentConsiderations
        .SelectMany(consideration => consideration.FundingCalculationOutput?.AllocationMatrix ?? [])
        .Sum(allocation => allocation.AllocatedAmount);
}
