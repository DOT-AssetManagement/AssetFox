using System;
using System.Collections.Generic;
using AnalysisEngine = AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using System.Linq;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class AllocationMapper
    {
        public static IEnumerable<Allocation> ToEntityList(List<AnalysisEngine.FundingCalculationOutput.Allocation> allocationMatrix, Guid fundingCalculationOutputId)
        {
            return allocationMatrix.Select(_ => new Allocation
            {
                Id = Guid.NewGuid(),
                AllocatedAmount = _.AllocatedAmount,
                BudgetName = _.BudgetName,
                FundingCalculationOutputId = fundingCalculationOutputId,
                TreatmentName = _.TreatmentName,
                Year = _.Year
            });
        }
    }
}
