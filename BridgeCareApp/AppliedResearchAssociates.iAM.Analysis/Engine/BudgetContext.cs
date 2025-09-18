using System;
using System.Collections.Generic;
using System.Linq;

namespace AppliedResearchAssociates.iAM.Analysis.Engine;

/// <summary>
///     Represents a budget evolving through the analysis period.
/// </summary>
internal sealed class BudgetContext
{
    private readonly decimal[] AmountPerYear;

    private readonly int FirstYearOfAnalysisPeriod;

    private int CurrentYearIndex = 0;

    public BudgetContext(Budget budget, int firstYearOfAnalysisPeriod)
    {
        Budget = budget ?? throw new ArgumentNullException(nameof(budget));
        FirstYearOfAnalysisPeriod = firstYearOfAnalysisPeriod;
        AmountPerYear = Budget.YearlyAmounts.Select(amount => amount.Value).ToArray();
    }

    public BudgetContext(BudgetContext original)
    {
        Budget = original.Budget;
        FirstYearOfAnalysisPeriod = original.FirstYearOfAnalysisPeriod;
        AmountPerYear = original.AmountPerYear.ToArray();

        CurrentYearIndex = original.CurrentYearIndex;
        CurrentPriorityAmount = original.CurrentPriorityAmount;
    }

    public Budget Budget { get; }

    public List<Action> CostDeallocations { get; set; }

    public decimal CurrentAmount => AmountPerYear[CurrentYearIndex];

    public decimal? CurrentPriorityAmount { get; private set; }

    public void AllocateCost(decimal cost, int targetYear) => _AllocateCost(cost, targetYear - FirstYearOfAnalysisPeriod);

    public decimal GetAmount(int year) => AmountPerYear[year - FirstYearOfAnalysisPeriod];

    public void MoveToNextYear()
    {
        ++CurrentYearIndex;
        if (CurrentYearIndex == AmountPerYear.Length)
        {
            throw new InvalidOperationException("Budget advanced beyond analysis period.");
        }

        SetPriority(null);

        if (Budget.InvestmentPlan.AllowFundingCarryover)
        {
            AmountPerYear[CurrentYearIndex] += AmountPerYear[CurrentYearIndex - 1];
            AmountPerYear[CurrentYearIndex - 1] = 0;
        }
    }

    public void SetPriority(BudgetPriority budgetPriority)
    {
        var priorityFraction = budgetPriority?.GetBudgetPercentagePair(Budget).Percentage / 100;
        CurrentPriorityAmount = CurrentAmount * priorityFraction;
    }

    private void _AllocateCost(decimal cost, int targetYearIndex)
    {
        if (cost < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cost));
        }

        if (targetYearIndex < CurrentYearIndex)
        {
            throw new ArgumentOutOfRangeException(nameof(targetYearIndex));
        }

        if (cost > 0)
        {
            if (Budget.InvestmentPlan.AllowFundingCarryover)
            {
                for (var yearIndex = targetYearIndex; yearIndex > CurrentYearIndex; --yearIndex)
                {
                    var availableAmount = AmountPerYear[yearIndex];
                    if (availableAmount > 0)
                    {
                        if (cost <= availableAmount)
                        {
                            allocateAndPrepareDeallocation(yearIndex, cost);
                            return;
                        }

                        allocateAndPrepareDeallocation(yearIndex, availableAmount);

                        cost -= availableAmount;
                        if (cost < 0)
                        {
                            throw new InvalidOperationException("Remaining cost is negative.");
                        }
                    }
                }

                allocateAndPrepareDeallocation(CurrentYearIndex, cost);

                allocateAndPrepareDeallocation(null, cost);
            }
            else
            {
                allocateAndPrepareDeallocation(targetYearIndex, cost);

                if (targetYearIndex == CurrentYearIndex)
                {
                    allocateAndPrepareDeallocation(null, cost);
                }
            }
        }

        void allocateAndPrepareDeallocation(int? yearIndex, decimal cost)
        {
            if (yearIndex is int i)
            {
                AmountPerYear[i] -= cost;
                CostDeallocations?.Add(() => AmountPerYear[i] += cost);
            }
            else
            {
                CurrentPriorityAmount -= cost;
                CostDeallocations?.Add(() => CurrentPriorityAmount += cost);
            }
        }
    }
}
