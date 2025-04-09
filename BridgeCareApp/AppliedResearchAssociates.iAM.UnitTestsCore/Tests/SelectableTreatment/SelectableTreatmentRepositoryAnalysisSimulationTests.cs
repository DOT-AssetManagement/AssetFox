using AppliedResearchAssociates.iAM.Data.Mappers;
using AppliedResearchAssociates.iAM.DataUnitTests.Tests;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Attributes;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Repositories;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.User;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using BridgeCareCoreTests.Tests;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Xunit;
using AppliedResearchAssociates.iAM.DataUnitTests;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests.SelectableTreatment
{
    public class SelectableTreatmentRepositoryAnalysisSimulationTests
    {
        [Fact]
        public async Task GetScenarioPerformanceCurvesWithAttributeNameLookup_SimulationInDbWithCurves_Gets()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
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
            var assets = new List<Data.Networking.MaintainableAsset> { asset };
            var dataNetwork = NetworkTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, assets, networkId, TestAttributeIds.BrKeyId, networkName);
            var assetIds = assets.Select(a => a.Id).ToList();
            var requiredAttributeIds = new List<Guid> { TestAttributeIds.AgeId };
            var simulationId = Guid.NewGuid();
            var simulationName = RandomStrings.WithPrefix("ExtremelyMinimalInput");
            var simulationModel = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId, simulationName, userId, networkId);
            var simulationAnalysisDetail = SimulationAnalysisDetailDtos.ForSimulation(simulationId);
            TestHelper.UnitOfWork.SimulationAnalysisDetailRepo.UpsertSimulationAnalysisDetail(simulationAnalysisDetail);
            var analysisMethod = AnalysisMethodDtos.RiskScore();
            TestHelper.UnitOfWork.AnalysisMethodRepo.UpsertAnalysisMethod(simulationId, analysisMethod);
            var investmentPlanDto = InvestmentPlanDtos.Dto(simulationId, 2024);
            TestHelper.UnitOfWork.InvestmentPlanRepo.UpsertInvestmentPlan(investmentPlanDto, simulationId);
            var noTreatmentDto = TreatmentDtos.NoTreatment();
            var treatments = new List<TreatmentDTO> { noTreatmentDto };
            TestHelper.UnitOfWork.SelectableTreatmentRepo.UpsertOrDeleteScenarioSelectableTreatment(treatments, simulationId);
            var network = TestHelper.UnitOfWork.NetworkRepo.GetSimulationAnalysisNetwork(networkId, explorer, true, simulationId);
            TestHelper.UnitOfWork.SimulationRepo.GetSimulationInNetwork(simulationId, network);
            var simulation = network.Simulations.Single(_ => _.Id == simulationId);

            TestHelper.UnitOfWork.SelectableTreatmentRepo.GetScenarioSelectableTreatments(simulation);

            var treatmentsAfter = simulation.Treatments.ToList();
            var treatmentAfter = treatmentsAfter.Single();
            Assert.Equal(noTreatmentDto.Id, treatmentAfter.Id);
            Assert.Equal(noTreatmentDto.Name, treatmentAfter.Name);
        }
    }
}
