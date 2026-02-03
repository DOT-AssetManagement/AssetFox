using System;
using System.Collections.Generic;
using System.Linq;
using AnalysisEngine = AssetFox.Core.Analysis.Engine;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class AllocationMapper
    {
        public static IEnumerable<Allocation> ToEntityList(List<AnalysisEngine.FundingCalculationOutput.Allocation> allocationMatrixDomainList, Guid fundingCalculationOutputId, int runId)
        {
            return allocationMatrixDomainList.Select(_ => new Allocation
            {
                Id = Guid.NewGuid(),
                AllocatedAmount = _.AllocatedAmount,
                BudgetName = _.BudgetName,
                FundingCalculationOutputId = fundingCalculationOutputId,
                TreatmentName = _.TreatmentName,
                Year = _.Year,
                RunId = runId
            });
        }

        public static List<AnalysisEngine.FundingCalculationOutput.Allocation> ToDomainList(ICollection<Allocation> allocationMatrixEntityCollection)
        {
            return allocationMatrixEntityCollection?.Select(_ => ToDomain(_)).ToList() ?? new();
        }

        private static AnalysisEngine.FundingCalculationOutput.Allocation ToDomain(Allocation allocationEntity)
        {
            return new AnalysisEngine.FundingCalculationOutput.Allocation(allocationEntity.Year, allocationEntity.BudgetName, allocationEntity.TreatmentName, allocationEntity.AllocatedAmount);
        }

        public static AllocationDTO ToDto(this Allocation entity)
        {
            var dto = new AllocationDTO
            {
                Id = entity.Id,
                AllocatedAmount = entity.AllocatedAmount,
                BudgetName = entity.BudgetName,
                TreatmentName = entity.TreatmentName,
                Year = entity.Year
            };

            return dto;
        }

        public static Allocation ToEntity(this AllocationDTO AllocationDto, Guid fundingCalculationOutputId)
        {
            return new Allocation
            {
                Id = AllocationDto.Id,
                FundingCalculationOutputId = fundingCalculationOutputId,
                AllocatedAmount = AllocationDto.AllocatedAmount,
                BudgetName = AllocationDto.BudgetName,
                TreatmentName = AllocationDto.TreatmentName,
                Year = AllocationDto.Year
            };
        }
    }
}
