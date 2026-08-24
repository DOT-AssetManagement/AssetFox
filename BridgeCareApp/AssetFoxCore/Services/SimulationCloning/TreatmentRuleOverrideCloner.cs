using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.SimulationCloning
{
    internal static class TreatmentRuleOverrideCloner
    {
        internal static List<TreatmentRuleOverrideDTO> CloneList(
            IEnumerable<TreatmentRuleOverrideDTO> overrides,
            IEnumerable<TreatmentDTO> sourceTreatments,
            IEnumerable<TreatmentDTO> clonedTreatments)
        {
            var sourceTreatmentList = (sourceTreatments ?? new List<TreatmentDTO>()).ToList();
            var clonedTreatmentList = (clonedTreatments ?? new List<TreatmentDTO>()).ToList();

            var clonedTreatmentsBySourceId = sourceTreatmentList
                .Zip(clonedTreatmentList, (source, clone) => new { Source = source, Clone = clone })
                .Where(pair => pair.Source.Id != Guid.Empty)
                .GroupBy(pair => pair.Source.Id)
                .ToDictionary(group => group.Key, group => group.First().Clone);

            return (overrides ?? new List<TreatmentRuleOverrideDTO>())
                .Select(ruleOverride => Clone(ruleOverride, clonedTreatmentsBySourceId))
                .ToList();
        }

        internal static List<TreatmentRuleOverrideDTO> CloneListForNewLibrary(
            IEnumerable<TreatmentRuleOverrideDTO> overrides,
            IEnumerable<TreatmentDTO> clonedTreatments)
        {
            var clonedTreatmentsBySourceId = (clonedTreatments ?? new List<TreatmentDTO>())
                .Where(treatment => treatment.SourceTreatmentId.HasValue)
                .GroupBy(treatment => treatment.SourceTreatmentId.Value)
                .ToDictionary(group => group.Key, group => group.First());

            return (overrides ?? new List<TreatmentRuleOverrideDTO>())
                .Select(ruleOverride => Clone(ruleOverride, clonedTreatmentsBySourceId))
                .ToList();
        }

        private static TreatmentRuleOverrideDTO Clone(
            TreatmentRuleOverrideDTO ruleOverride,
            Dictionary<Guid, TreatmentDTO> clonedTreatmentsBySourceId)
        {
            return new TreatmentRuleOverrideDTO
            {
                Id = Guid.NewGuid(),
                Name = ruleOverride.Name,
                Description = ruleOverride.Description,
                SourceTreatmentRuleOverrideId = ruleOverride.SourceTreatmentRuleOverrideId,
                IsEnabledByDefault = ruleOverride.IsEnabledByDefault,
                IsEnabled = ruleOverride.IsEnabled,
                Criteria = (ruleOverride.Criteria ?? new List<TreatmentRuleOverrideCriterionDTO>())
                    .Select(criterion => CloneCriterion(criterion, clonedTreatmentsBySourceId))
                    .Where(criterion => criterion.TreatmentIds.Any())
                    .ToList()
            };
        }

        private static TreatmentRuleOverrideCriterionDTO CloneCriterion(
            TreatmentRuleOverrideCriterionDTO criterion,
            Dictionary<Guid, TreatmentDTO> clonedTreatmentsBySourceId)
        {
            var clonedTreatmentIds = new List<Guid>();
            var clonedTreatmentDtos = new List<SimpleTreatmentDTO>();

            foreach (var sourceTreatmentId in GetTreatmentIds(criterion).Distinct())
            {
                if (!clonedTreatmentsBySourceId.TryGetValue(sourceTreatmentId, out var clonedTreatment))
                {
                    continue;
                }

                clonedTreatmentIds.Add(clonedTreatment.Id);
                clonedTreatmentDtos.Add(new SimpleTreatmentDTO
                {
                    Id = clonedTreatment.Id,
                    Name = clonedTreatment.Name,
                    IsUnselectable = clonedTreatment.IsUnselectable
                });
            }

            return new TreatmentRuleOverrideCriterionDTO
            {
                Id = Guid.NewGuid(),
                LogicalOperator = criterion.LogicalOperator,
                SortOrder = criterion.SortOrder,
                CriterionExpression = criterion.CriterionExpression,
                TreatmentIds = clonedTreatmentIds.Distinct().ToList(),
                Treatments = clonedTreatmentDtos
                    .GroupBy(t => t.Id)
                    .Select(g => g.First())
                    .OrderBy(t => t.Name)
                    .ToList()
            };
        }

        private static IEnumerable<Guid> GetTreatmentIds(TreatmentRuleOverrideCriterionDTO criterion) =>
            (criterion.TreatmentIds ?? new List<Guid>())
                .Concat((criterion.Treatments ?? new List<SimpleTreatmentDTO>()).Select(treatment => treatment.Id))
                .Where(id => id != Guid.Empty);
    }
}
