using System;
using System.Collections.Generic;
using System.Linq;
using AppliedResearchAssociates.iAM.Analysis;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.DTOs.Enums;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Benefit;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Repositories;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Xunit;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests
{
    public class AnalysisMethodRepositoryTests
    {
        [Fact]
        public void GetAnalysisMethod_AnalysisMethodInDb_Gets()
        {
            var unitOfWork = TestHelper.UnitOfWork;
            AttributeTestSetup.CreateAttributes(unitOfWork);
            NetworkTestSetup.CreateNetwork(unitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var entity = AnalysisMethodEntities.TestAnalysis(simulation.Id);
            TestHelper.UnitOfWork.Context.AnalysisMethod.Add(entity);
            TestHelper.UnitOfWork.Context.SaveChanges();
            // Act
            var result = unitOfWork.AnalysisMethodRepo.GetAnalysisMethod(simulation.Id);
            // Assert
            Assert.NotNull(result);
        }


        [Fact]
        public void UpsertAnalysisMethod_AnalysisMethodAlreadyInDb_UpdatesBenefit()
        {
            // Arrange
            var unitOfWork = TestHelper.UnitOfWork;
            AttributeTestSetup.CreateAttributes(unitOfWork);
            NetworkTestSetup.CreateNetwork(unitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var repo = unitOfWork.AnalysisMethodRepo;
            var analysisMethodDto = repo.GetAnalysisMethod(simulation.Id);
            analysisMethodDto.Benefit = BenefitDtos.Dto(TestAttributeNames.Age);

            // Act
            repo.UpsertAnalysisMethod(simulation.Id, analysisMethodDto);

            // Assert
            var upsertedAnalysisMethodDto = repo.GetAnalysisMethod(simulation.Id);
            Assert.Equal(analysisMethodDto.Id, upsertedAnalysisMethodDto.Id);
            Assert.Equal(analysisMethodDto.Benefit.Id, upsertedAnalysisMethodDto.Benefit.Id);
        }

        [Fact]
        public void GetAnalysisMethodSetting_AnalysisMethodDoesNotExist_False()
        {
            var unitOfWork = TestHelper.UnitOfWork;
            AttributeTestSetup.CreateAttributes(unitOfWork);
            NetworkTestSetup.CreateNetwork(unitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var repo = unitOfWork.AnalysisMethodRepo;

            var analysisMethodSetting = unitOfWork.AnalysisMethodRepo.GetSimulationAnalysisMethodSetting(simulation.Id);

            Assert.False(analysisMethodSetting);
        }

        [Fact]
        public void GetAnalysisMethodSetting_AnalysisMethodExists_True()
        {
            var unitOfWork = TestHelper.UnitOfWork;
            AttributeTestSetup.CreateAttributes(unitOfWork);
            NetworkTestSetup.CreateNetwork(unitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var repo = unitOfWork.AnalysisMethodRepo;
            var analysisMethodDto = repo.GetAnalysisMethod(simulation.Id);
            analysisMethodDto.Benefit = BenefitDtos.Dto(TestAttributeNames.Age);

            // Act
            repo.UpsertAnalysisMethod(simulation.Id, analysisMethodDto);

            var analysisMethodSetting = unitOfWork.AnalysisMethodRepo.GetSimulationAnalysisMethodSetting(simulation.Id);

            Assert.True(analysisMethodSetting);

        }

        [Fact]
        public void UpsertAnalysisMethod_AnalysisMethodInDb_Updates()
        {
            var unitOfWork = TestHelper.UnitOfWork;
            AttributeTestSetup.CreateAttributes(unitOfWork);
            NetworkTestSetup.CreateNetwork(unitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var criterionLibrary = CriterionLibraryTestSetup.TestCriterionLibraryInDb(TestHelper.UnitOfWork);
            var repo = unitOfWork.AnalysisMethodRepo;

            var analysisMethodDto = repo.GetAnalysisMethod(simulation.Id);
            var attributeEntity = TestHelper.UnitOfWork.Context.Attribute.First();
            analysisMethodDto.Attribute = attributeEntity.Name;
            analysisMethodDto.CriterionLibrary = criterionLibrary;
            var analysisMethod = AnalysisMethodEntities.TestAnalysis(simulation.Id);
            var benefitDto = BenefitDtos.Dto(attributeEntity.Name);
            analysisMethodDto.Benefit = benefitDto;

            // Act
            repo.UpsertAnalysisMethod(simulation.Id, analysisMethodDto);

            // Assert
            var analysisMethodDtoAfter = repo.GetAnalysisMethod(simulation.Id);

            Assert.Equal(analysisMethodDtoAfter.Id, analysisMethodDto.Id);
            Assert.Equal(analysisMethodDtoAfter.Attribute, analysisMethodDto.Attribute);
            Assert.Equal(analysisMethodDtoAfter.CriterionLibrary.Id, analysisMethodDto.CriterionLibrary.Id);
            Assert.Equal(analysisMethodDtoAfter.Benefit.Id, analysisMethodDto.Benefit.Id);
            Assert.Equal(analysisMethodDtoAfter.Benefit.Attribute, analysisMethodDto.Benefit.Attribute);
        }

        [Fact]
        public void GetSimulationAnalysisMethod_SimulationInDbWithChildren_Gets()
        {
            // wjwjwj working on this test
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var simulation = SimulationTestSetup.DomainSimulation(TestHelper.UnitOfWork);
            var simulationId = simulation.Id;
            var budgetId = Guid.NewGuid();
            var budgetName = RandomStrings.WithPrefixAnd2CharSuffix("Budget");
            var budgetDto = BudgetDtos.New(budgetId, budgetName);
            var budgetDtos = new List<BudgetDTO> { budgetDto };
            var simulationAnalysisDetail = SimulationAnalysisDetailDtos.ForSimulation(simulationId);
            TestHelper.UnitOfWork.SimulationAnalysisDetailRepo.UpsertSimulationAnalysisDetail(simulationAnalysisDetail);
            var analysisMethod = AnalysisMethodDtos.RiskScore();
            TestHelper.UnitOfWork.AnalysisMethodRepo.UpsertAnalysisMethod(simulationId, analysisMethod);
            var investmentPlanDto = InvestmentPlanDtos.Dto(simulationId, 2024);
            TestHelper.UnitOfWork.InvestmentPlanRepo.UpsertInvestmentPlan(investmentPlanDto, simulationId); var curveId = Guid.NewGuid(); TestHelper.UnitOfWork.BudgetRepo.UpsertOrDeleteScenarioBudgets(budgetDtos, simulation.Id);

            var budgetPriorityId = Guid.NewGuid();
            var budgetPercentagePairId = Guid.NewGuid();
            var budgetPercentagePairDto = new BudgetPercentagePairDTO
            {
                Percentage = 33,
                Id = budgetPercentagePairId,
                BudgetId = budgetId,
                BudgetName = budgetName,
                
            };
            var budgetPriorityDto = new BudgetPriorityDTO
            {
                Id = budgetPriorityId,
                BudgetPercentagePairs = new List<BudgetPercentagePairDTO> { budgetPercentagePairDto },
                PriorityLevel = 0,
                Year = 2025,
            };
            var budgetPriorityDtos = new List<BudgetPriorityDTO> { budgetPriorityDto };
            TestHelper.UnitOfWork.BudgetPriorityRepo.UpsertOrDeleteScenarioBudgetPriorities(budgetPriorityDtos, simulation.Id);
            var description = RandomStrings.WithPrefixAnd2CharSuffix("Description");
            var criterionLibraryDescription = RandomStrings.WithPrefixAnd2CharSuffix("CriterionLibraryDescription");
            var analysisMethodId = Guid.NewGuid();
            var criterionLibraryId = Guid.NewGuid();
            var benefitId = Guid.NewGuid();
            var benefit = new BenefitDTO
            {
                Attribute = TestAttributeNames.ConditionIndex,
                Id = benefitId,
                Limit = 45,
            };
            var criterionLibrary = new CriterionLibraryDTO
            {
                Id = criterionLibraryId,
                Description = criterionLibraryDescription,
                MergedCriteriaExpression = "mergedCriteriaDescription",
            };
            var analysisMethodDto = new AnalysisMethodDTO
            {
                Benefit = benefit,
                CriterionLibrary = criterionLibrary,
                Description = description,
                Id = analysisMethodId,
                OptimizationStrategy = OptimizationStrategy.RemainingLife,
                ShouldAllowMultipleTreatments = true,
                ShouldApplyMultipleFeasibleCosts = true,
                ShouldDeteriorateDuringCashFlow = true,
                ShouldUseExtraFundsAcrossBudgets = true,
                SpendingStrategy = SpendingStrategy.UnlimitedSpending,                
            };
            TestHelper.UnitOfWork.AnalysisMethodRepo.UpsertAnalysisMethod(simulation.Id, analysisMethodDto);
            TestHelper.UnitOfWork.InvestmentPlanRepo.GetSimulationInvestmentPlan(simulation);

            TestHelper.UnitOfWork.AnalysisMethodRepo.GetSimulationAnalysisMethod(simulation, "");

            var analysisMethodAfter = simulation.AnalysisMethod;
            Assert.Equal(analysisMethodId, analysisMethodAfter.Id);
            Assert.Equal(description, analysisMethodAfter.Description);
            Assert.Equal(OptimizationStrategy.RemainingLife, analysisMethodAfter.OptimizationStrategy);
            Assert.Equal(SpendingStrategy.UnlimitedSpending, analysisMethodAfter.SpendingStrategy);
            Assert.True(analysisMethodAfter.ShouldApplyMultipleFeasibleCosts);
            Assert.True(analysisMethodAfter.ShouldDeteriorateDuringCashFlow);
            Assert.True(analysisMethodAfter.AllowFundingFromMultipleBudgets);
            Assert.True(simulation.ShouldBundleFeasibleTreatments);
            var benefitAfter = analysisMethodAfter.Benefit;
            Assert.Equal(45, benefitAfter.Limit);
            Assert.Equal(benefitId, benefitAfter.Id);
            Assert.Equal(TestAttributeNames.ConditionIndex, benefitAfter.Attribute.Name);
            var budgetPriorityAfter = analysisMethodAfter.BudgetPriorities.Single();
            Assert.Equal(budgetPriorityDto.Id, budgetPriorityAfter.Id);
            var percentagePairAfter = budgetPriorityAfter.BudgetPercentagePairs.Single();
            Assert.Equal(33m, percentagePairAfter.Percentage);
            Assert.Equal(budgetPercentagePairId, percentagePairAfter.Id);
            Assert.Equal(budgetName, percentagePairAfter.Budget.Name);
            Assert.Equal(budgetId, percentagePairAfter.Budget.Id);
        }
    }
}
