using AppliedResearchAssociates.iAM.DTOs;
using System.Collections.Generic;
using System;

namespace BridgeCareCore.Services.SimulationCloning
{
    public class TreatmentSchedulingCollisionDetailCloner
    {
        internal static IList<TreatmentSchedulingCollisionDetailDTO> CloneList(IList<TreatmentSchedulingCollisionDetailDTO> treatmentSchedulingCollisions)
        {
            var cloneList = new List<TreatmentSchedulingCollisionDetailDTO>();

            foreach (var treatmentSchedulingCollision in treatmentSchedulingCollisions)
            {
                cloneList.Add(Clone(treatmentSchedulingCollision));
            }

            return cloneList;
        }

        internal static TreatmentSchedulingCollisionDetailDTO Clone(TreatmentSchedulingCollisionDetailDTO treatmentSchedulingCollision)
        {
            return new TreatmentSchedulingCollisionDetailDTO
            {
                Id = Guid.NewGuid(),
                NameOfUnscheduledTreatment = treatmentSchedulingCollision.NameOfUnscheduledTreatment
            };
        }
    }
}
