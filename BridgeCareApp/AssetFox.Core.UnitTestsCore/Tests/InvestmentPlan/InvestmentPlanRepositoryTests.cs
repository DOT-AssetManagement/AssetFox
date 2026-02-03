using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.TestUtils;
using Xunit;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public class InvestmentPlanRepositoryTests
    {
        [Fact]
        public void UpsertInvestmentPlan_SimulationInDb_Inserts()
        {
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var investmentPlan = new InvestmentPlanDTO
            {
                Id = Guid.NewGuid(),
            };

            TestHelper.UnitOfWork.InvestmentPlanRepo.UpsertInvestmentPlan(investmentPlan, simulation.Id);

            var investmentPlanAfter = TestHelper.UnitOfWork.InvestmentPlanRepo.GetInvestmentPlan(simulation.Id);
            ObjectAssertions.Equivalent(investmentPlan, investmentPlanAfter);
        }

        [Fact]
        public void GetSimulationInvestmentPlan_Does()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var simulation = SimulationTestSetup.DomainSimulation(TestHelper.UnitOfWork);
            var investmentPlanId = Guid.NewGuid();
            var investmentPlanDto = new InvestmentPlanDTO
            {
                FirstYearOfAnalysisPeriod = 2025,
                NumberOfYearsInAnalysisPeriod = 2,
                Id = investmentPlanId,
                InflationRatePercentage = 6,
                MinimumProjectCostLimit = 123456m,
                ShouldAccumulateUnusedBudgetAmounts = true,
            };
            TestHelper.UnitOfWork.InvestmentPlanRepo.UpsertInvestmentPlan(investmentPlanDto, simulation.Id);

            TestHelper.UnitOfWork.InvestmentPlanRepo.GetSimulationInvestmentPlan(simulation);

            var simulationInvestmentPlan = simulation.InvestmentPlan;
            Assert.Equal(investmentPlanId, simulationInvestmentPlan.Id);
            Assert.Equal(6, simulationInvestmentPlan.InflationRatePercentage);
            Assert.Equal(2025, simulationInvestmentPlan.FirstYearOfAnalysisPeriod);
            Assert.True(simulationInvestmentPlan.AllowFundingCarryover);
            Assert.Equal(123456m, simulationInvestmentPlan.MinimumProjectCostLimit);
            Assert.Equal(2, simulationInvestmentPlan.NumberOfYearsInAnalysisPeriod);
            Assert.Equal(2026, simulationInvestmentPlan.LastYearOfAnalysisPeriod);
            var expectedYearsOfAnalysis = new List<int> { 2025, 2026 };
            var actualYearsOfAnalysis = simulationInvestmentPlan.YearsOfAnalysis;
            ObjectAssertions.Equivalent(expectedYearsOfAnalysis, actualYearsOfAnalysis);
        }
    }
}
