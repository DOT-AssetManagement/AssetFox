using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.SimulationCloning
{
    public class AllocationCloner
    {
        internal static IList<AllocationDTO> CloneList(IList<AllocationDTO> allocationMatrix)
        {
            var cloneList = new List<AllocationDTO>();

            foreach (var allocation in allocationMatrix)
            {
                cloneList.Add(Clone(allocation));
            }

            return cloneList;
        }

        private static AllocationDTO Clone(AllocationDTO allocation)
        {
            return new AllocationDTO
            {
                Id = Guid.NewGuid(),
                Year = allocation.Year,
                AllocatedAmount = allocation.AllocatedAmount,
                BudgetName = allocation.BudgetName,
                TreatmentName = allocation.TreatmentName
            };
        }
    }
}
