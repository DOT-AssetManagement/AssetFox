using System;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class InvestmentPlanDtos
    {
        public static InvestmentPlanDTO Dto(
            Guid? id = null,
            int firstYearOfAnalysisPeriod = 2022,
            int numberOfYearsInAnalysisPeriod = 1)
        {
            var resolveId = id ?? Guid.NewGuid();
            var dto = new InvestmentPlanDTO
            {
                FirstYearOfAnalysisPeriod = firstYearOfAnalysisPeriod,
                Id = resolveId,
                InflationRatePercentage = 3,
                NumberOfYearsInAnalysisPeriod = numberOfYearsInAnalysisPeriod,
                MinimumProjectCostLimit = 500000,
            };
            return dto;
        }

    }
}
