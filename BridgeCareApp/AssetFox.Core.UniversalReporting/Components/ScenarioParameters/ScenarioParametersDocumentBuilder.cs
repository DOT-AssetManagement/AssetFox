using System.Globalization;
using System.Text.RegularExpressions;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.UniversalReporting.Components.ScenarioParameters;

public sealed class ScenarioParametersSourceData
{
    public SimulationDTO? Simulation { get; init; }
    public InvestmentPlanDTO? InvestmentPlan { get; init; }
    public AnalysisMethodDTO? AnalysisMethod { get; init; }
    public IReadOnlyList<BudgetDTO> Budgets { get; init; } = Array.Empty<BudgetDTO>();
    public IReadOnlyList<BudgetPriorityDTO> BudgetPriorities { get; init; } = Array.Empty<BudgetPriorityDTO>();
    public IReadOnlyList<CashFlowRuleDTO> CashFlowRules { get; init; } = Array.Empty<CashFlowRuleDTO>();
    public IReadOnlyList<TreatmentDTO> Treatments { get; init; } = Array.Empty<TreatmentDTO>();
    public IReadOnlyList<BaseCommittedProjectDTO> CommittedProjects { get; init; } = Array.Empty<BaseCommittedProjectDTO>();
}

public static partial class ScenarioParametersDocumentBuilder
{
    public static ScenarioParametersDocument Build(
        ScenarioParametersSourceData source,
        IEnumerable<string>? selectedSections = null)
    {
        var selected = new HashSet<string>(
            selectedSections ?? ScenarioParameterSectionKeys.All,
            StringComparer.OrdinalIgnoreCase);
        var document = new ScenarioParametersDocument();

        AddIfSelected(document, selected, BuildScenarioSection(source.Simulation, source.AnalysisMethod));
        AddIfSelected(document, selected, BuildInvestmentSection(source.InvestmentPlan));
        AddIfSelected(document, selected, BuildAnalysisSection(source.Simulation, source.AnalysisMethod));
        AddIfSelected(document, selected, BuildScenarioCriteriaSection(source.AnalysisMethod));
        AddIfSelected(document, selected, BuildPrioritiesSection(source.BudgetPriorities));
        AddIfSelected(document, selected, BuildCashFlowSection(source.CashFlowRules));
        AddIfSelected(document, selected, BuildFundingSection(source.Budgets));
        AddIfSelected(document, selected, BuildBudgetCriteriaSection(source.Budgets));
        AddIfSelected(document, selected, BuildTreatmentsSection(source.Treatments, source.CommittedProjects));

        return document;
    }

    private static ScenarioParameterSection BuildScenarioSection(
        SimulationDTO? simulation,
        AnalysisMethodDTO? analysisMethod)
    {
        var section = KeyValueSection(ScenarioParameterSectionKeys.Scenario, "Scenario", 10);
        if (simulation == null)
        {
            return section;
        }

        AddKeyValue(section, "name", "Scenario Name", simulation.Name);
        AddKeyValue(section, "description", "Description", analysisMethod?.Description);
        AddKeyValue(section, "network", "Network", simulation.NetworkName);
        AddKeyValue(section, "owner", "Owner", simulation.Owner);
        AddKeyValue(section, "creator", "Created By", simulation.Creator);
        AddKeyValue(section, "created", "Created", simulation.CreatedDate, "date");
        AddKeyValue(section, "modified", "Last Modified", simulation.LastModifiedDate, "date");
        AddKeyValue(section, "lastRun", "Last Run", simulation.LastRun, "date");
        return section;
    }

    private static ScenarioParameterSection BuildInvestmentSection(InvestmentPlanDTO? investmentPlan)
    {
        var section = KeyValueSection(ScenarioParameterSectionKeys.Investment, "Investment", 20);
        if (investmentPlan == null)
        {
            return section;
        }

        AddKeyValue(section, "startYear", "Start Year", investmentPlan.FirstYearOfAnalysisPeriod, "year");
        AddKeyValue(section, "analysisPeriod", "Analysis Period", investmentPlan.NumberOfYearsInAnalysisPeriod, "number");
        AddKeyValue(section, "inflationRate", "Inflation Rate", investmentPlan.InflationRatePercentage, "percent");
        AddKeyValue(section, "minimumProjectCost", "Minimum Project Cost", investmentPlan.MinimumProjectCostLimit, "currency");
        AddKeyValue(section, "carryUnusedFunding", "Carry Unused Funding Forward", investmentPlan.ShouldAccumulateUnusedBudgetAmounts, "boolean");
        return section;
    }

    private static ScenarioParameterSection BuildAnalysisSection(
        SimulationDTO? simulation,
        AnalysisMethodDTO? analysisMethod)
    {
        var section = KeyValueSection(ScenarioParameterSectionKeys.Analysis, "Analysis", 30);
        if (analysisMethod == null)
        {
            return section;
        }

        AddKeyValue(section, "optimization", "Optimization Strategy", Humanize(analysisMethod.OptimizationStrategy.ToString()));
        AddKeyValue(section, "spending", "Spending Strategy", Humanize(analysisMethod.SpendingStrategy.ToString()));
        AddKeyValue(section, "weighting", "Weighting Attribute", analysisMethod.Attribute);
        AddKeyValue(section, "benefitAttribute", "Benefit Attribute", analysisMethod.Benefit?.Attribute);
        if (analysisMethod.Benefit != null)
        {
            AddKeyValue(section, "benefitLimit", "Benefit Limit", analysisMethod.Benefit.Limit, "number");
        }

        AddKeyValue(section, "multipleFeasibleCosts", "Apply Multiple Feasible Costs", analysisMethod.ShouldApplyMultipleFeasibleCosts, "boolean");
        AddKeyValue(section, "deteriorateDuringCashFlow", "Deteriorate During Cash Flow", analysisMethod.ShouldDeteriorateDuringCashFlow, "boolean");
        AddKeyValue(section, "crossBudgetFunding", "Allow Remaining Funds Across Budgets", analysisMethod.ShouldUseExtraFundsAcrossBudgets, "boolean");
        AddKeyValue(section, "multipleTreatments", "Allow Multiple Treatments", analysisMethod.ShouldAllowMultipleTreatments, "boolean");
        AddKeyValue(section, "superAssets", "Use Super Assets", analysisMethod.ShouldUseSuperAssets, "boolean");
        AddKeyValue(section, "superAssetAttribute", "Super Asset Attribute", analysisMethod.SuperAssetAttribute);
        if (simulation != null)
        {
            AddKeyValue(section, "noTreatmentBeforeCommitted", "No Treatments Before Committed Projects", simulation.NoTreatmentBeforeCommittedProjects, "boolean");
        }

        return section;
    }

    private static ScenarioParameterSection BuildScenarioCriteriaSection(AnalysisMethodDTO? analysisMethod)
    {
        var section = KeyValueSection(ScenarioParameterSectionKeys.ScenarioCriteria, "Scenario Criteria", 40);
        AddKeyValue(section, "criteria", "Criteria", analysisMethod?.CriterionLibrary?.MergedCriteriaExpression);
        return section;
    }

    private static ScenarioParameterSection BuildPrioritiesSection(IReadOnlyList<BudgetPriorityDTO> priorities)
    {
        var section = TableSection(
            ScenarioParameterSectionKeys.Priorities,
            "Analysis Priorities",
            50,
            ("priority", "Priority"),
            ("year", "Year"),
            ("criteria", "Criteria"),
            ("allocations", "Budget Allocations"));

        var rowOrder = 0;
        foreach (var priority in priorities
                     .OrderBy(priority => priority.PriorityLevel)
                     .ThenBy(priority => priority.Year ?? int.MinValue))
        {
            AddTableRow(section, $"priority-{rowOrder}", rowOrder++,
                ("priority", priority.PriorityLevel, "number"),
                ("year", priority.Year?.ToString(CultureInfo.InvariantCulture) ?? "All years", "text"),
                ("criteria", priority.CriterionLibrary?.MergedCriteriaExpression ?? string.Empty, "text"),
                ("allocations", FormatBudgetAllocations(priority.BudgetPercentagePairs), "text"));
        }

        return section;
    }

    private static ScenarioParameterSection BuildCashFlowSection(IReadOnlyList<CashFlowRuleDTO> cashFlowRules)
    {
        var section = TableSection(
            ScenarioParameterSectionKeys.CashFlow,
            "Cash Flow Rules",
            60,
            ("ruleSet", "Rule Set"),
            ("criteria", "Criteria"),
            ("duration", "Duration"),
            ("costCeiling", "Maximum Project Cost"),
            ("percentages", "Yearly Percentages"));

        var rowOrder = 0;
        foreach (var ruleSet in cashFlowRules.OrderBy(rule => rule.Name, StringComparer.OrdinalIgnoreCase))
        {
            foreach (var rule in (ruleSet.CashFlowDistributionRules ?? new List<CashFlowDistributionRuleDTO>())
                         .OrderBy(rule => rule.CostCeiling)
                         .ThenBy(rule => rule.DurationInYears))
            {
                AddTableRow(section, $"cash-flow-{rowOrder}", rowOrder++,
                    ("ruleSet", ruleSet.Name, "text"),
                    ("criteria", ruleSet.CriterionLibrary?.MergedCriteriaExpression ?? string.Empty, "text"),
                    ("duration", rule.DurationInYears, "number"),
                    ("costCeiling", rule.CostCeiling, "currency"),
                    ("percentages", rule.YearlyPercentages, "text"));
            }
        }

        return section;
    }

    private static ScenarioParameterSection BuildFundingSection(IReadOnlyList<BudgetDTO> budgets)
    {
        var section = new ScenarioParameterSection
        {
            Key = ScenarioParameterSectionKeys.Funding,
            Title = "Funding by Year",
            Kind = ScenarioParameterSectionKinds.Matrix,
            Order = 70,
            RowHeader = "Year"
        };

        var orderedBudgets = budgets
            .Where(budget => !string.IsNullOrWhiteSpace(budget.Name))
            .GroupBy(budget => budget.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => new
            {
                Name = group.Key,
                Order = group.Max(budget => budget.BudgetOrder),
                Budgets = group.ToList()
            })
            .OrderByDescending(budget => budget.Order)
            .ThenBy(budget => budget.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        for (var columnOrder = 0; columnOrder < orderedBudgets.Count; columnOrder++)
        {
            section.Columns.Add(new ScenarioParameterColumn
            {
                Key = $"budget-{columnOrder}",
                Label = orderedBudgets[columnOrder].Name,
                Order = columnOrder
            });
        }

        var years = orderedBudgets
            .SelectMany(budget => budget.Budgets)
            .SelectMany(budget => budget.BudgetAmounts ?? new List<BudgetAmountDTO>())
            .Select(amount => amount.Year)
            .Distinct()
            .OrderBy(year => year)
            .ToList();

        for (var rowOrder = 0; rowOrder < years.Count; rowOrder++)
        {
            var year = years[rowOrder];
            var row = new ScenarioParameterRow
            {
                Key = $"year-{year}",
                Label = year.ToString(CultureInfo.InvariantCulture),
                Order = rowOrder
            };

            for (var columnOrder = 0; columnOrder < orderedBudgets.Count; columnOrder++)
            {
                var budget = orderedBudgets[columnOrder];
                var matchingAmounts = budget.Budgets
                    .SelectMany(item => item.BudgetAmounts ?? new List<BudgetAmountDTO>())
                    .Where(amount => amount.Year == year)
                    .ToList();
                object? value = matchingAmounts.Count == 0
                    ? null
                    : matchingAmounts.Sum(amount => amount.Value);

                row.Cells.Add(new ScenarioParameterCell
                {
                    ColumnKey = $"budget-{columnOrder}",
                    Value = value,
                    Format = "currency"
                });
            }

            section.Rows.Add(row);
        }

        return section;
    }

    private static ScenarioParameterSection BuildBudgetCriteriaSection(IReadOnlyList<BudgetDTO> budgets)
    {
        var section = TableSection(
            ScenarioParameterSectionKeys.BudgetCriteria,
            "Budget Criteria",
            80,
            ("order", "Funding Order"),
            ("budget", "Budget"),
            ("criteria", "Criteria"));

        var rowOrder = 0;
        foreach (var budget in budgets
                     .OrderByDescending(budget => budget.BudgetOrder)
                     .ThenBy(budget => budget.Name, StringComparer.OrdinalIgnoreCase))
        {
            AddTableRow(section, $"budget-{rowOrder}", rowOrder++,
                ("order", budget.BudgetOrder, "number"),
                ("budget", budget.Name, "text"),
                ("criteria", budget.CriterionLibrary?.MergedCriteriaExpression ?? string.Empty, "text"));
        }

        return section;
    }

    private static ScenarioParameterSection BuildTreatmentsSection(
        IReadOnlyList<TreatmentDTO> treatments,
        IReadOnlyList<BaseCommittedProjectDTO> committedProjects)
    {
        var section = TableSection(
            ScenarioParameterSectionKeys.Treatments,
            "Treatments",
            90,
            ("source", "Source"),
            ("name", "Treatment"),
            ("category", "Category"),
            ("criteria", "Criteria"));

        var rowOrder = 0;
        foreach (var treatment in treatments
                     .Where(treatment => !string.IsNullOrWhiteSpace(treatment.Name))
                     .GroupBy(treatment => treatment.Name, StringComparer.OrdinalIgnoreCase)
                     .Select(group => group.First())
                     .OrderBy(treatment => treatment.Name, StringComparer.OrdinalIgnoreCase))
        {
            AddTableRow(section, $"selectable-{rowOrder}", rowOrder++,
                ("source", "Selectable", "text"),
                ("name", treatment.Name, "text"),
                ("category", Humanize(treatment.Category.ToString()), "text"),
                ("criteria", treatment.CriterionLibrary?.MergedCriteriaExpression ?? string.Empty, "text"));
        }

        foreach (var project in committedProjects
                     .Where(project => !string.IsNullOrWhiteSpace(project.ComputedTreatmentString))
                     .GroupBy(project => project.ComputedTreatmentString, StringComparer.OrdinalIgnoreCase)
                     .Select(group => group.First())
                     .OrderBy(project => project.ComputedTreatmentString, StringComparer.OrdinalIgnoreCase))
        {
            AddTableRow(section, $"committed-{rowOrder}", rowOrder++,
                ("source", "Committed Project", "text"),
                ("name", project.ComputedTreatmentString, "text"),
                ("category", Humanize(project.Category.ToString()), "text"),
                ("criteria", string.Empty, "text"));
        }

        return section;
    }

    private static ScenarioParameterSection KeyValueSection(string key, string title, int order)
    {
        var section = new ScenarioParameterSection
        {
            Key = key,
            Title = title,
            Kind = ScenarioParameterSectionKinds.KeyValue,
            Order = order
        };
        section.Columns.Add(new ScenarioParameterColumn { Key = "value", Label = "Value", Order = 0 });
        return section;
    }

    private static ScenarioParameterSection TableSection(
        string key,
        string title,
        int order,
        params (string Key, string Label)[] columns)
    {
        var section = new ScenarioParameterSection
        {
            Key = key,
            Title = title,
            Kind = ScenarioParameterSectionKinds.Table,
            Order = order
        };
        for (var i = 0; i < columns.Length; i++)
        {
            section.Columns.Add(new ScenarioParameterColumn
            {
                Key = columns[i].Key,
                Label = columns[i].Label,
                Order = i
            });
        }

        return section;
    }

    private static void AddKeyValue(
        ScenarioParameterSection section,
        string key,
        string label,
        object? value,
        string format = "text")
    {
        if (value == null || value is string text && string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var row = new ScenarioParameterRow
        {
            Key = key,
            Label = label,
            Order = section.Rows.Count
        };
        row.Cells.Add(new ScenarioParameterCell { ColumnKey = "value", Value = value, Format = format });
        section.Rows.Add(row);
    }

    private static void AddTableRow(
        ScenarioParameterSection section,
        string key,
        int order,
        params (string ColumnKey, object? Value, string Format)[] cells)
    {
        var row = new ScenarioParameterRow { Key = key, Order = order };
        foreach (var cell in cells)
        {
            row.Cells.Add(new ScenarioParameterCell
            {
                ColumnKey = cell.ColumnKey,
                Value = cell.Value,
                Format = cell.Format
            });
        }

        section.Rows.Add(row);
    }

    private static void AddIfSelected(
        ScenarioParametersDocument document,
        ISet<string> selected,
        ScenarioParameterSection section)
    {
        if (selected.Contains(section.Key) && section.Rows.Count > 0)
        {
            document.Sections.Add(section);
        }
    }

    private static string FormatBudgetAllocations(IReadOnlyCollection<BudgetPercentagePairDTO>? pairs)
    {
        if (pairs == null || pairs.Count == 0)
        {
            return string.Empty;
        }

        return string.Join("; ", pairs
            .OrderBy(pair => pair.BudgetName, StringComparer.OrdinalIgnoreCase)
            .Select(pair => $"{pair.BudgetName}: {pair.Percentage:0.##}%"));
    }

    private static string Humanize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return PascalCaseBoundary().Replace(value.Replace('_', ' '), " $1").Trim();
    }

    [GeneratedRegex("(?<=[a-z0-9])([A-Z])")]
    private static partial Regex PascalCaseBoundary();
}
