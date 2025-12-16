using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.SimulationCloning
{
    public class TreatmentRejectionDetailCloner
    {
        internal static IList<TreatmentRejectionDetailDTO> CloneList(IList<TreatmentRejectionDetailDTO> treatmentRejections)
        {
            var cloneList = new List<TreatmentRejectionDetailDTO>();

            foreach (var treatmentRejection in treatmentRejections)
            {
                cloneList.Add(Clone(treatmentRejection));
            }

            return cloneList;
        }

        internal static TreatmentRejectionDetailDTO Clone(TreatmentRejectionDetailDTO treatmentRejection)
        {
            return new TreatmentRejectionDetailDTO
            {
                Id = Guid.NewGuid(),
                TreatmentName = treatmentRejection.TreatmentName,
                PotentialConditionChange = treatmentRejection.PotentialConditionChange,
                TreatmentRejectionReason = treatmentRejection.TreatmentRejectionReason
            };
        }
    }
}
