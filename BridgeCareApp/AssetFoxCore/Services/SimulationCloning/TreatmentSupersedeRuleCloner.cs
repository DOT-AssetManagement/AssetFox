using AssetFox.Core.DTOs;
using System.Collections.Generic;
using System;
using System.Linq;

namespace AssetFoxCore.Services
{
    public class TreatmentSupersedeRuleCloner
    {
        internal static TreatmentSupersedeRuleDTO Clone(TreatmentSupersedeRuleDTO treatmentSupersedeRule, TreatmentDTO preventSupersedeRule, Guid ownerId)
        {
            var cloneCriterionLibrary = CriterionLibraryCloner.CloneNullPropagating(treatmentSupersedeRule.CriterionLibrary, ownerId);            
            var cloneTreatmentSupersedeRule = new TreatmentSupersedeRuleDTO
            {
                Id = Guid.NewGuid(),                
                CriterionLibrary = cloneCriterionLibrary,
                treatment = preventSupersedeRule
            };
            return cloneTreatmentSupersedeRule;
        }

        internal static List<TreatmentSupersedeRuleDTO> CloneList(
            IEnumerable<TreatmentSupersedeRuleDTO> treatmentSupersedeRules,
            IReadOnlyDictionary<Guid, TreatmentDTO> clonedTreatmentsBySourceId,
            Guid ownerId)
        {
            var cloneList = new List<TreatmentSupersedeRuleDTO>();
            foreach (var treatmentSupersedeRule in treatmentSupersedeRules ?? new List<TreatmentSupersedeRuleDTO>())
            {
                if (treatmentSupersedeRule.treatment == null
                    || !clonedTreatmentsBySourceId.TryGetValue(treatmentSupersedeRule.treatment.Id, out var preventSupersedeRule))
                {
                    continue;
                }

                var cloneTreatmentSupersedeRule = Clone(treatmentSupersedeRule, preventSupersedeRule, ownerId);
                cloneList.Add(cloneTreatmentSupersedeRule);
            }
            return cloneList;
        }
    }
}
