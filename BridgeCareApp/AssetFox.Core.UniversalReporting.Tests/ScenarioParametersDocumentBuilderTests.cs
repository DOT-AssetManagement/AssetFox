using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Enums;
using AssetFox.Core.UniversalReporting.Components.ScenarioParameters;
using Xunit;

namespace AssetFox.Core.UniversalReporting.Tests;

public class ScenarioParametersDocumentBuilderTests
{
    [Fact]
    public void Build_CreatesGenericScenarioSectionsAndFundingMatrix()
    {
        var source = CreateSource();

        var document = ScenarioParametersDocumentBuilder.Build(source);

        Assert.Equal(ScenarioParameterSectionKeys.All, document.Sections.Select(section => section.Key));

        var scenario = Assert.Single(document.Sections, section => section.Key == ScenarioParameterSectionKeys.Scenario);
        Assert.Contains(scenario.Rows, row => row.Label == "Scenario Name"
            && Equals(row.Cells.Single().Value, "Capital Plan"));

        var funding = Assert.Single(document.Sections, section => section.Key == ScenarioParameterSectionKeys.Funding);
        Assert.Equal("Year", funding.RowHeader);
        Assert.Equal(new[] { "Capital", "Maintenance" }, funding.Columns.Select(column => column.Label));
        Assert.Equal(new[] { "2027", "2028" }, funding.Rows.Select(row => row.Label));
        Assert.Equal(2_000_000m, funding.Rows[0].Cells[0].Value);
        Assert.Equal(500_000m, funding.Rows[0].Cells[1].Value);

        var treatments = Assert.Single(document.Sections, section => section.Key == ScenarioParameterSectionKeys.Treatments);
        Assert.Contains(treatments.Rows, row => row.Cells.Any(cell => Equals(cell.Value, "Rehabilitation")));
        Assert.Contains(treatments.Rows, row => row.Cells.Any(cell => Equals(cell.Value, "Emergency Repair")));

        var structuralLabels = document.Sections
            .SelectMany(section => new[] { section.Title, section.RowHeader }
                .Concat(section.Columns.Select(column => column.Label))
                .Concat(section.Rows.Select(row => row.Label)))
            .ToList();
        Assert.DoesNotContain(structuralLabels, label =>
            label.Contains("NHS", StringComparison.OrdinalIgnoreCase)
            || label.Contains("BPN", StringComparison.OrdinalIgnoreCase)
            || label.Contains("BAMS", StringComparison.OrdinalIgnoreCase)
            || label.Contains("PAMS", StringComparison.OrdinalIgnoreCase)
            || label.Contains("MPMS", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Build_OnlyIncludesSelectedSections()
    {
        var document = ScenarioParametersDocumentBuilder.Build(
            CreateSource(),
            new[] { ScenarioParameterSectionKeys.Investment, ScenarioParameterSectionKeys.Funding });

        Assert.Equal(
            new[] { ScenarioParameterSectionKeys.Investment, ScenarioParameterSectionKeys.Funding },
            document.Sections.Select(section => section.Key));
    }

    [Fact]
    public void DataContract_RoundTripsStructuredSections()
    {
        var original = ScenarioParametersDocumentBuilder.Build(
            CreateSource(),
            new[] { ScenarioParameterSectionKeys.Scenario, ScenarioParameterSectionKeys.Funding });

        var rows = ScenarioParametersDataContract.Encode(original).ToList();
        var decoded = ScenarioParametersDataContract.Decode(ScenarioParametersDataContract.Columns, rows);

        Assert.Equal(original.Sections.Select(section => section.Key), decoded.Sections.Select(section => section.Key));
        Assert.Equal(
            original.Sections.SelectMany(section => section.Rows).Count(),
            decoded.Sections.SelectMany(section => section.Rows).Count());
        Assert.Equal("Capital", decoded.Sections.Single(section => section.Key == ScenarioParameterSectionKeys.Funding).Columns[0].Label);
    }

    private static ScenarioParametersSourceData CreateSource()
    {
        var simulationId = Guid.NewGuid();
        return new ScenarioParametersSourceData
        {
            Simulation = new SimulationDTO
            {
                Id = simulationId,
                Name = "Capital Plan",
                NetworkName = "Statewide Network",
                Owner = "Planner",
                Creator = "Administrator",
                CreatedDate = new DateTime(2026, 1, 2),
                LastModifiedDate = new DateTime(2026, 2, 3),
                LastRun = new DateTime(2026, 3, 4),
                NoTreatmentBeforeCommittedProjects = true
            },
            InvestmentPlan = new InvestmentPlanDTO
            {
                FirstYearOfAnalysisPeriod = 2027,
                NumberOfYearsInAnalysisPeriod = 2,
                InflationRatePercentage = 2.5,
                MinimumProjectCostLimit = 10_000m,
                ShouldAccumulateUnusedBudgetAmounts = true
            },
            AnalysisMethod = new AnalysisMethodDTO
            {
                Description = "Two-year capital plan",
                OptimizationStrategy = OptimizationStrategy.BenefitToCostRatio,
                SpendingStrategy = SpendingStrategy.AsBudgetPermits,
                Attribute = "Condition Index",
                Benefit = new BenefitDTO { Attribute = "Condition Index", Limit = 20 },
                CriterionLibrary = new CriterionLibraryDTO { MergedCriteriaExpression = "Condition Index < 80" },
                ShouldApplyMultipleFeasibleCosts = true,
                ShouldDeteriorateDuringCashFlow = true,
                ShouldUseExtraFundsAcrossBudgets = true,
                ShouldAllowMultipleTreatments = false,
                ShouldUseSuperAssets = false
            },
            Budgets = new List<BudgetDTO>
            {
                new()
                {
                    Name = "Maintenance",
                    BudgetOrder = 1,
                    CriterionLibrary = new CriterionLibraryDTO { MergedCriteriaExpression = "Cost < 100000" },
                    BudgetAmounts = new List<BudgetAmountDTO>
                    {
                        new() { Year = 2027, Value = 500_000m },
                        new() { Year = 2028, Value = 600_000m }
                    }
                },
                new()
                {
                    Name = "Capital",
                    BudgetOrder = 2,
                    CriterionLibrary = new CriterionLibraryDTO { MergedCriteriaExpression = "Cost >= 100000" },
                    BudgetAmounts = new List<BudgetAmountDTO>
                    {
                        new() { Year = 2027, Value = 2_000_000m },
                        new() { Year = 2028, Value = 2_500_000m }
                    }
                }
            },
            BudgetPriorities = new List<BudgetPriorityDTO>
            {
                new()
                {
                    PriorityLevel = 1,
                    CriterionLibrary = new CriterionLibraryDTO { MergedCriteriaExpression = "Risk > 90" },
                    BudgetPercentagePairs = new List<BudgetPercentagePairDTO>
                    {
                        new() { BudgetName = "Capital", Percentage = 50 }
                    }
                }
            },
            CashFlowRules = new List<CashFlowRuleDTO>
            {
                new()
                {
                    Name = "Major Projects",
                    CriterionLibrary = new CriterionLibraryDTO { MergedCriteriaExpression = "Cost > 1000000" },
                    CashFlowDistributionRules = new List<CashFlowDistributionRuleDTO>
                    {
                        new() { DurationInYears = 2, CostCeiling = 5_000_000m, YearlyPercentages = "50/50" }
                    }
                }
            },
            Treatments = new List<TreatmentDTO>
            {
                new()
                {
                    Name = "Rehabilitation",
                    Category = TreatmentCategory.Rehabilitation,
                    CriterionLibrary = new CriterionLibraryDTO { MergedCriteriaExpression = "Condition Index < 60" }
                }
            },
            CommittedProjects = new List<SectionCommittedProjectDTO>
            {
                new()
                {
                    SimulationId = simulationId,
                    Treatment = new[] { "Emergency Repair" },
                    Category = TreatmentCategory.Maintenance
                }
            }
        };
    }
}
