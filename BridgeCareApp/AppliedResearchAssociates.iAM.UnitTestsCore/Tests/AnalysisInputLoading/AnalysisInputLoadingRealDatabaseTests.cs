using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.Analysis;
using AppliedResearchAssociates.iAM.Data.Mappers;
using AppliedResearchAssociates.iAM.Data.Networking;
using AppliedResearchAssociates.iAM.DataPersistenceCore;
using AppliedResearchAssociates.iAM.DataUnitTests.Tests;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Attributes;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Repositories;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.SelectableTreatment;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.User;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using BridgeCareCoreTests.Tests;
using Xunit;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests
{
    public class AnalysisInputLoadingRealDatabaseTests
    {
        [Fact]
        public async Task ExtremelyMinimalInput()
        {
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, true);
            var userId = user.Id;
            var userInfo = UserInfoDtos.ForUser(user);
            TestHelper.UnitOfWork.UserCriteriaRepo.GetOwnUserCriteria(userInfo);
            TestHelper.UnitOfWork.SetUser(user.Username);
            var dataSource = AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var explorer = TestHelper.UnitOfWork.AttributeRepo.GetExplorer();
            var networkName = RandomStrings.WithPrefix("minimalInputNetwork");
            var networkId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var keyAttributeName = TestAttributeNames.BrKey;
            var asset = MaintainableAssets.InNetwork(networkId, keyAttributeName, assetId);
            var assets = new List<MaintainableAsset> { asset };
            var network = NetworkTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, assets, networkId, TestAttributeIds.BrKeyId, networkName);
            var assetIds = assets.Select(a => a.Id).ToList();
            var requiredAttributeIds = new List<Guid> { TestAttributeIds.AgeId };
            var simulationId = Guid.NewGuid();
            var simulationName = RandomStrings.WithPrefix("ExtremelyMinimalInput");
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId, simulationName, userId, networkId);
            var simulationAnalysisDetail = SimulationAnalysisDetailDtos.ForSimulation(simulationId);
            TestHelper.UnitOfWork.SimulationAnalysisDetailRepo.UpsertSimulationAnalysisDetail(simulationAnalysisDetail);
            var analysisMethod = AnalysisMethodDtos.RiskScore();
            TestHelper.UnitOfWork.AnalysisMethodRepo.UpsertAnalysisMethod(simulationId, analysisMethod);
            var investmentPlanDto = InvestmentPlanDtos.Dto(simulationId, 2024);
            TestHelper.UnitOfWork.InvestmentPlanRepo.UpsertInvestmentPlan(investmentPlanDto, simulationId);
            var noTreatmentDto = TreatmentDtos.NoTreatment();
            var treatments = new List<TreatmentDTO> { noTreatmentDto };
            TestHelper.UnitOfWork.SelectableTreatmentRepo.UpsertOrDeleteScenarioSelectableTreatment(treatments, simulationId);
            var budgetId = Guid.NewGuid();
            var budgetName = RandomStrings.WithPrefix("Budget");
            var budget = BudgetDtos.WithSingleAmount(budgetId, budgetName, 2024, 1000000m);
            var budgets = new List<BudgetDTO> { budget };
            TestHelper.UnitOfWork.BudgetRepo.AddScenarioBudgets(simulationId, budgets);
            var budgetAmountWithBudgetId = new BudgetAmountDTOWithBudgetId
            {
                BudgetAmount = budget.BudgetAmounts.Single(),
                BudgetId = budgetId,
            };
            var budgetAmountsWithBudgetIds = new List<BudgetAmountDTOWithBudgetId> { budgetAmountWithBudgetId };
            TestHelper.UnitOfWork.BudgetRepo.AddScenarioBudgetAmounts(budgetAmountsWithBudgetIds);
            var budgetPriority = BudgetPriorityDtos.WithPercentagePair(budgetName, budgetId, null, 0, 2024);
            var budgetPriorities = new List<BudgetPriorityDTO> { budgetPriority };
            TestHelper.UnitOfWork.BudgetPriorityRepo.UpsertOrDeleteScenarioBudgetPriorities(budgetPriorities, simulationId);
            var conditionIndexAttribute = TestHelper.UnitOfWork.AttributeRepo.GetSingleById(TestAttributeIds.ConditionIndexId);
            var ageAttribute = TestHelper.UnitOfWork.AttributeRepo.GetSingleById(TestAttributeIds.AgeId);
            var ageAttributeAsList = new List<AttributeDTO> { ageAttribute };
            var mappedAttributeList = AttributeDtoDomainMapper.ToDomainList(ageAttributeAsList, "");
            AggregatedResultTestSetup.AddNumericAggregatedResultsToDb(TestHelper.UnitOfWork, assets, mappedAttributeList, 30);
            var calculatedAttribute = CalculatedAttributeDtos.ForAttribute(conditionIndexAttribute);
            var calculatedAttributeEquation = calculatedAttribute.Equations.Single();
            calculatedAttributeEquation.Equation.Expression = "100 - [AGE]";
            var calculatedAttributes = new List<CalculatedAttributeDTO> { calculatedAttribute };
            TestHelper.UnitOfWork.CalculatedAttributeRepo.UpsertScenarioCalculatedAttributesNonAtomic(calculatedAttributes, simulationId);
            var input = GetSimulationInput(networkId, simulationId);
            Assert.Equal(simulationId, input.Id);
            Assert.Equal(budgetId, input.InvestmentPlan.Budgets.Single().Id);
            Assert.Equal(investmentPlanDto.Id, input.InvestmentPlan.Id);
            Assert.Equal(noTreatmentDto.Id, input.DesignatedPassiveTreatment.Id);
            Assert.Equal(analysisMethod.Id, input.AnalysisMethod.Id);
            Assert.Equal(budgetPriority.Id, input.AnalysisMethod.BudgetPriorities.Single().Id);
            Assert.Single(input.InvestmentPlan.BudgetConditions);
            Assert.Equal(input.InvestmentPlan.Budgets.Single().YearlyAmounts.Single().Id, budgetAmountWithBudgetId.BudgetAmount.Id);
            Assert.Equal(assetId, input.Network.Assets.Single().Id);
            Assert.Single(input.Network.Explorer.CalculatedFields.Where(cf => cf.Name == TestAttributeNames.ConditionIndex));
        }

        private static Simulation GetSimulationInput(Guid networkId, Guid simulationId)
        {
            Func<bool> returnTrue = () => true;
            var input = AnalysisInputLoading.GetSimulationWithAssets(TestHelper.UnitOfWork, networkId, simulationId,
                returnTrue,
                returnTrue,
                returnTrue,
                returnTrue,
                returnTrue,
                returnTrue,
                returnTrue,
                returnTrue);
            return input;
        }
    }
}
