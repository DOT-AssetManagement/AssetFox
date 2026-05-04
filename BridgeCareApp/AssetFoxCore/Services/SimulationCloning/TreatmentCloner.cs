using AssetFox.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetFoxCore.Services
{
    internal class TreatmentCloner
    {
        internal static TreatmentDTO Clone(
            TreatmentDTO treatment,
            Guid ownerId,
            IReadOnlyDictionary<Guid, BudgetDTO> clonedBudgetsBySourceId)
        {            
            var cloneCriterionLibrary = CriterionLibraryCloner.CloneNullPropagating(treatment.CriterionLibrary, ownerId);
            var cloneTreatmentCost = TreatmentCostCloner.CloneList(treatment.Costs, ownerId);
            var cloneTreatmentConsequence = TreatmentConsequenceCloner.CloneList(treatment.Consequences, ownerId);            
            var clonedBudgets = CloneTreatmentBudgets(treatment, clonedBudgetsBySourceId);
            var clone = new TreatmentDTO
            {
              Name = treatment.Name,
              LibraryId = treatment.LibraryId,
              Description = treatment.Description,
              ShadowForAnyTreatment = treatment.ShadowForAnyTreatment,
              ShadowForSameTreatment = treatment.ShadowForSameTreatment,
              CriterionLibrary = cloneCriterionLibrary,
              AssetType = treatment.AssetType,
              BudgetIds = clonedBudgets.Select(budget => budget.Id).ToList(),
              Budgets = clonedBudgets,
              Category = treatment.Category,
              Consequences = cloneTreatmentConsequence,
              Costs = cloneTreatmentCost,
              Id = Guid.NewGuid(),
              IsModified = treatment.IsModified,
              PerformanceFactors = treatment.PerformanceFactors,
              SupersedeRules = new List<TreatmentSupersedeRuleDTO>(),
              IsUnselectable = treatment.IsUnselectable,
            }; 
            return clone;
        }

        internal static List<TreatmentDTO> CloneList(
            IEnumerable<TreatmentDTO> treatments,
            Guid ownerId,
            IReadOnlyDictionary<Guid, BudgetDTO> clonedBudgetsBySourceId)
        {
            var sourceTreatments = (treatments ?? new List<TreatmentDTO>()).ToList();
            var cloneList = sourceTreatments
                .Select(treatment => Clone(treatment, ownerId, clonedBudgetsBySourceId))
                .ToList();

            var clonedTreatmentsBySourceId = sourceTreatments
                .Zip(cloneList, (source, clone) => new { Source = source, Clone = clone })
                .Where(pair => pair.Source.Id != Guid.Empty)
                .GroupBy(pair => pair.Source.Id)
                .ToDictionary(group => group.Key, group => group.First().Clone);

            // Clone supersede rules here(to get correct prevent treatment ids assigned for rules
            foreach (var treatmentPair in sourceTreatments.Zip(cloneList, (source, clone) => new { Source = source, Clone = clone }))
            {
                if (treatmentPair.Source.SupersedeRules != null && treatmentPair.Source.SupersedeRules.Any())
                {
                    var cloneSupersedeRules = TreatmentSupersedeRuleCloner.CloneList(treatmentPair.Source.SupersedeRules, clonedTreatmentsBySourceId, ownerId);
                    treatmentPair.Clone.SupersedeRules = cloneSupersedeRules;
                }
            }

            return cloneList;
        }

        private static List<TreatmentBudgetDTO> CloneTreatmentBudgets(
            TreatmentDTO treatment,
            IReadOnlyDictionary<Guid, BudgetDTO> clonedBudgetsBySourceId)
        {
            if (clonedBudgetsBySourceId == null)
            {
                return treatment.Budgets ?? new List<TreatmentBudgetDTO>();
            }

            var sourceBudgetIds = (treatment.BudgetIds ?? new List<Guid>())
                .Concat((treatment.Budgets ?? new List<TreatmentBudgetDTO>()).Select(budget => budget.Id))
                .Where(id => id != Guid.Empty)
                .Distinct();

            return sourceBudgetIds
                .Where(clonedBudgetsBySourceId.ContainsKey)
                .Select(sourceBudgetId =>
                {
                    var clonedBudget = clonedBudgetsBySourceId[sourceBudgetId];
                    return new TreatmentBudgetDTO
                    {
                        Id = clonedBudget.Id,
                        Name = clonedBudget.Name
                    };
                })
                .ToList();
        }
    }
}
