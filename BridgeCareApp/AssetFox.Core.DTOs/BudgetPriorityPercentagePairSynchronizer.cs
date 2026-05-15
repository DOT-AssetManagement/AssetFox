using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetFox.Core.DTOs
{
    public static class BudgetPriorityPercentagePairSynchronizer
    {
        private const decimal DefaultPercentage = 100;

        public static void SynchronizeWithBudgets(
            IEnumerable<BudgetPriorityDTO> budgetPriorities,
            IEnumerable<SimpleBudgetDetailDTO> currentBudgets)
        {
            var budgets = currentBudgets?.ToList() ?? new List<SimpleBudgetDetailDTO>();
            if (budgetPriorities == null)
            {
                return;
            }

            foreach (var budgetPriority in budgetPriorities)
            {
                budgetPriority.BudgetPercentagePairs = SynchronizePairs(
                    budgetPriority.BudgetPercentagePairs,
                    budgets);
            }
        }

        public static List<BudgetPercentagePairDTO> SynchronizePairs(
            IEnumerable<BudgetPercentagePairDTO> existingPairs,
            IEnumerable<SimpleBudgetDetailDTO> currentBudgets)
        {
            var pairs = existingPairs?.ToList() ?? new List<BudgetPercentagePairDTO>();
            var budgets = currentBudgets?.ToList() ?? new List<SimpleBudgetDetailDTO>();
            var currentBudgetIds = budgets.Select(_ => _.Id).ToHashSet();
            var uniqueCurrentBudgetNames = budgets
                .GroupBy(_ => _.Name)
                .Where(_ => _.Key != null && _.Count() == 1)
                .ToDictionary(_ => _.Key, _ => _.Single());

            var synchronizedPairs = new List<BudgetPercentagePairDTO>();

            foreach (var budget in budgets)
            {
                var matchingPair = pairs.FirstOrDefault(_ => _.BudgetId == budget.Id);
                matchingPair ??= FindUnambiguousNameFallbackPair(
                    pairs,
                    budget,
                    currentBudgetIds,
                    uniqueCurrentBudgetNames);

                synchronizedPairs.Add(matchingPair == null
                    ? CreateDefaultPair(budget)
                    : CreatePairForCurrentBudget(matchingPair, budget));
            }

            return synchronizedPairs;
        }

        private static BudgetPercentagePairDTO FindUnambiguousNameFallbackPair(
            List<BudgetPercentagePairDTO> pairs,
            SimpleBudgetDetailDTO budget,
            HashSet<Guid> currentBudgetIds,
            Dictionary<string, SimpleBudgetDetailDTO> uniqueCurrentBudgetNames)
        {
            if (budget.Name == null || !uniqueCurrentBudgetNames.ContainsKey(budget.Name))
            {
                return null;
            }

            var stalePairsWithMatchingName = pairs
                .Where(_ => _.BudgetName == budget.Name && !currentBudgetIds.Contains(_.BudgetId))
                .ToList();

            return stalePairsWithMatchingName.Count == 1
                ? stalePairsWithMatchingName.Single()
                : null;
        }

        private static BudgetPercentagePairDTO CreateDefaultPair(SimpleBudgetDetailDTO budget) =>
            new BudgetPercentagePairDTO
            {
                Id = Guid.NewGuid(),
                BudgetId = budget.Id,
                BudgetName = budget.Name,
                Percentage = DefaultPercentage,
            };

        private static BudgetPercentagePairDTO CreatePairForCurrentBudget(
            BudgetPercentagePairDTO source,
            SimpleBudgetDetailDTO budget) =>
            new BudgetPercentagePairDTO
            {
                Id = source.Id == Guid.Empty ? Guid.NewGuid() : source.Id,
                BudgetId = budget.Id,
                BudgetName = budget.Name,
                Percentage = source.Percentage,
            };
    }
}
