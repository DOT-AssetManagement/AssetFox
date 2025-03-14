using System;
using System.Collections.Generic;
using System.Linq;
using AnalysisEngine = AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class AllocationMapper
    {
        public static IEnumerable<Allocation> ToEntityList(List<AnalysisEngine.FundingCalculationOutput.Allocation> allocationMatrixDomainList, Guid fundingCalculationOutputId)
        {
            return allocationMatrixDomainList.Select(_ => new Allocation
            {
                Id = Guid.NewGuid(),
                AllocatedAmount = _.AllocatedAmount,
                BudgetName = _.BudgetName,
                FundingCalculationOutputId = fundingCalculationOutputId,
                TreatmentName = _.TreatmentName,
                Year = _.Year
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
    }
}
