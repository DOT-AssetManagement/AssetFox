using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs;

namespace BridgeCareCore.Services.SimulationCloning
{
    public class TreatmentOptionDetailCloner
    {
        internal static IList<TreatmentOptionDetailDTO> CloneList(IList<TreatmentOptionDetailDTO> treatmentOptions)
        {
            var cloneList = new List<TreatmentOptionDetailDTO>();

            foreach (var treatmentOption in treatmentOptions)
            {
                cloneList.Add(Clone(treatmentOption));
            }

            return cloneList;
        }

        internal static TreatmentOptionDetailDTO Clone(TreatmentOptionDetailDTO treatmentOption)
        {
            return new TreatmentOptionDetailDTO
            {
                Id = Guid.NewGuid(),
                Benefit = treatmentOption.Benefit,
                ConditionChange = treatmentOption.ConditionChange,
                Cost = treatmentOption.Cost,
                RemainingLife = treatmentOption.RemainingLife,
                TreatmentName = treatmentOption.TreatmentName
            };
        }
    }
}
