using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs;

namespace BridgeCareCore.Services.SimulationCloning
{
    public class TreatmentConsiderationDetailCloner
    {
        internal static IList<TreatmentConsiderationDetailDTO> CloneList(IList<TreatmentConsiderationDetailDTO> treatmentConsiderations)
        {
            var cloneList = new List<TreatmentConsiderationDetailDTO>();

            foreach (var treatmentConsideration in treatmentConsiderations)
            {
                cloneList.Add(Clone(treatmentConsideration));
            }

            return cloneList;
        }

        internal static TreatmentConsiderationDetailDTO Clone(TreatmentConsiderationDetailDTO treatmentConsideration)
        {
            var cloneCashFlowConsiderations = CashFlowConsiderationDetailCloner.CloneList(treatmentConsideration.CashFlowConsiderations);
            var cloneFundingCalculationInput = FundingCalculationInputCloner.Clone(treatmentConsideration.FundingCalculationInput);
            var cloneFundingCalculationOutput = FundingCalculationOutputCloner.Clone(treatmentConsideration.FundingCalculationOutput);

            return new TreatmentConsiderationDetailDTO
            {
                Id = Guid.NewGuid(),
                TreatmentName = treatmentConsideration.TreatmentName,
                BudgetPriorityLevel = treatmentConsideration.BudgetPriorityLevel,
                CashFlowConsiderations = cloneCashFlowConsiderations,
                FundingCalculationInput = cloneFundingCalculationInput,
                FundingCalculationOutput = cloneFundingCalculationOutput
            };
        }
    }
}
