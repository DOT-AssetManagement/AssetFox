using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.Analysis;
using AppliedResearchAssociates.iAM.Analysis.Input.DataTransfer;
using AppliedResearchAssociates.iAM.Data.Mappers;
using AppliedResearchAssociates.iAM.Data.Networking;
using AppliedResearchAssociates.iAM.DataPersistenceCore;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DataUnitTests.Tests;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Attributes;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Repositories;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.SelectableTreatment;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.User;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using BridgeCareCoreTests.Tests;
using MoreLinq;
using Xunit;
using Assert = Xunit.Assert;
using DataNetworkingNetwork = AppliedResearchAssociates.iAM.Data.Networking.Network;

namespace AppliedResearchAssociates.iAM.UnitTestsCore
{
    public class PerformanceCurveRepositoryTests
    {


        private void Setup()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
        }

        [Fact]
        public async Task GetScenarioPerformanceCurvesWithAttributeNameLookup_SimulationInDbWithCurves_Gets()
        {
            // The version of this test in commit d9c661 in the history
            // shows how to setup a Simulation for load/run
            // (but without actually running it).
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
            var curveId = Guid.NewGuid();
            var performanceCurve = ScenarioPerformanceCurveTestSetup.DtoForEntityInDb(TestHelper.UnitOfWork, simulationId, curveId);
            var network = TestHelper.UnitOfWork.NetworkRepo.GetSimulationAnalysisNetwork(networkId, explorer, true, simulationId);
            TestHelper.UnitOfWork.SimulationRepo.GetSimulationInNetwork(simulationId, network);
            var simulation = network.Simulations.Single(_ => _.Id == simulationId);
            var attributeNameLookup = TestHelper.UnitOfWork.AttributeRepo.GetAttributeNameLookupDictionary();

            TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulation, attributeNameLookup);

            var curvesAfter = simulation.PerformanceCurves.ToList();
            var curveAfter = curvesAfter.Single();
            Assert.Equal(performanceCurve.Name, curveAfter.Name);
            Assert.Equal(curveId, curveAfter.Id);
            Assert.Equal(performanceCurve.Attribute, curveAfter.Attribute.Name);
        }

        [Fact]
        public void UpsertOrDeleteScenarioPerformanceCurves_CurveInDb_UpdatesShift()
        {
            Setup();
            var simulationId = Guid.NewGuid();
            var curveId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
            var performanceCurve = ScenarioPerformanceCurveTestSetup.DtoForEntityInDb(TestHelper.UnitOfWork, simulationId, curveId);
            var scenarioCurves = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            var performanceCurveDto = scenarioCurves[0];
            performanceCurveDto.Shift = true;
            var UpdateRows = new List<PerformanceCurveDTO> { performanceCurveDto };

            // Act
            TestHelper.UnitOfWork.PerformanceCurveRepo.UpsertOrDeleteScenarioPerformanceCurvesNonAtomic(UpdateRows, simulation.Id);

            // Assert
            var performanceCurveLibraryDtoAfter = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            var performanceCurveDtoAfter = performanceCurveLibraryDtoAfter.Single();
            Assert.Equal(performanceCurveDto.Shift, performanceCurveDtoAfter.Shift);
            Assert.Equal(performanceCurveDto.Attribute, performanceCurveDtoAfter.Attribute);
        }

        [Fact]
        public void UpsertOrDeleteScenarioPerformanceCurves_CurveInDbWithEquation_EquationUnchanged()
        {
            Setup();
            // Arrange
            var simulationId = Guid.NewGuid();
            var curveId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
            var performanceCurve = ScenarioPerformanceCurveTestSetup.DtoForEntityInDb(TestHelper.UnitOfWork, simulationId, curveId, equation: "2");
            var updateRows = new List<PerformanceCurveDTO> { performanceCurve };

            // Act
            TestHelper.UnitOfWork.PerformanceCurveRepo.UpsertOrDeleteScenarioPerformanceCurvesNonAtomic(updateRows, simulation.Id);

            // Assert
            var performanceCurveLibraryDtoAfter = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            var performanceCurveDtoAfter = performanceCurveLibraryDtoAfter.Single();
            Assert.Equal(performanceCurve.Equation.Id, performanceCurveDtoAfter.Equation.Id);
            Assert.Equal(performanceCurve.Attribute, performanceCurveDtoAfter.Attribute);
        }

        [Fact]
        public void UpsertOrDeleteScenarioPerformanceCurves_CurveInDbWithEquation_UpdateRemovesEquationFromCurve_EquationRemoved()
        {
            Setup();
            // Arrange
            var simulationId = Guid.NewGuid();
            var curveId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
            var performanceCurve = ScenarioPerformanceCurveTestSetup.DtoForEntityInDb(TestHelper.UnitOfWork, simulationId, curveId, equation: "2");
            var equationId = performanceCurve.Equation.Id;
            var scenarioCurves = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            var performanceCurveDto = scenarioCurves[0];
            performanceCurveDto.Equation = null;
            var updateRows = new List<PerformanceCurveDTO> { performanceCurveDto };

            // Act
            TestHelper.UnitOfWork.PerformanceCurveRepo.UpsertOrDeleteScenarioPerformanceCurvesNonAtomic(updateRows, simulation.Id);

            // Assert
            var performanceCurveLibraryDtoAfter = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            var performanceCurveDtoAfter = performanceCurveLibraryDtoAfter.Single();
            Assert.Null(performanceCurveDtoAfter.Equation?.Expression);
            Assert.Equal(performanceCurveDto.Attribute, performanceCurveDtoAfter.Attribute);
            var equationAfter = TestHelper.UnitOfWork.Context.Equation.SingleOrDefault(e => e.Id == equationId);
            Assert.Null(equationAfter);
        }

        [Fact]
        public void UpsertOrDeleteScenarioPerformanceCurves_CurveInDbWithCriterionLibrary_CriterionLibraryUnchanged()
        {
            Setup();
            // Arrange
            var simulationId = Guid.NewGuid();
            var curveId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
            var criterionLibrary = CriterionLibraryTestSetup.TestCriterionLibrary();
            var performanceCurve = ScenarioPerformanceCurveTestSetup.DtoForEntityInDb(TestHelper.UnitOfWork, simulationId, curveId, criterionLibrary);
            var scenarioCurves = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            var performanceCurveDto = scenarioCurves[0];
            var updateRows = new List<PerformanceCurveDTO> { performanceCurveDto };
            // Act
            TestHelper.UnitOfWork.PerformanceCurveRepo.UpsertOrDeleteScenarioPerformanceCurvesNonAtomic(updateRows, simulation.Id);

            // Assert
            var scenarioCurvesAfter = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            var performanceCurveDtoAfter = scenarioCurvesAfter[0];
            Assert.Equal(performanceCurve.CriterionLibrary.Id, performanceCurveDtoAfter.CriterionLibrary.Id);
            Assert.Equal(performanceCurveDto.Attribute, performanceCurveDtoAfter.Attribute);
        }

        [Fact]
        public void UpsertOrDeleteScenarioPerformanceCurves_CurveInDbWithCriterionLibrary_UpdateRemovesCriterionLibraryFromCurve_CriterionLibraryRemoved()
        {
            Setup();
            // Arrange
            var simulationId = Guid.NewGuid();
            var curveId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
            var criterionLibrary = CriterionLibraryTestSetup.TestCriterionLibrary();
            var performanceCurve = ScenarioPerformanceCurveTestSetup.DtoForEntityInDb(TestHelper.UnitOfWork, simulationId, curveId, criterionLibrary);
            var scenarioCurves = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            var performanceCurveDto = scenarioCurves[0];
            performanceCurveDto.CriterionLibrary = null;

            var updateRows = new List<PerformanceCurveDTO> { performanceCurveDto };

            // Act
            TestHelper.UnitOfWork.PerformanceCurveRepo.UpsertOrDeleteScenarioPerformanceCurvesNonAtomic(updateRows, simulation.Id);

            // Assert
            var scenarioCurvesAfter = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            var performanceCurveDtoAfter = scenarioCurvesAfter[0];
            Assert.Equal(Guid.Empty, performanceCurveDtoAfter.CriterionLibrary.Id);
            Assert.Equal(performanceCurveDto.Attribute, performanceCurveDtoAfter.Attribute);
            var criterionLibraryJoinAfter = TestHelper.UnitOfWork.Context.CriterionLibraryPerformanceCurve.SingleOrDefault(clpc =>
            clpc.PerformanceCurveId == curveId
            && clpc.CriterionLibraryId == criterionLibrary.Id);
            Assert.Null(criterionLibraryJoinAfter);
        }

        [Fact]
        public void UpsertOrDeleteScenarioPerformanceCurves_CurveInDbWithCriterionLibrary_CurveRemovedFromUpsertedLibrary_RemovesCurveAndCriterionJoin()
        {
            Setup();
            // Arrange
            var simulationId = Guid.NewGuid();
            var curveId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
            var criterionLibrary = CriterionLibraryTestSetup.TestCriterionLibrary();
            var performanceCurve = ScenarioPerformanceCurveTestSetup.DtoForEntityInDb(TestHelper.UnitOfWork, simulationId, curveId);
            var scenarioCurves = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);

            var updateRows = new List<PerformanceCurveDTO>();

            // Act
            TestHelper.UnitOfWork.PerformanceCurveRepo.UpsertOrDeleteScenarioPerformanceCurvesNonAtomic(updateRows, simulation.Id);

            // Assert
            var scenarioCurvesAfter = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            Assert.Empty(scenarioCurvesAfter);
            var criterionLibraryJoinAfter = TestHelper.UnitOfWork.Context.CriterionLibraryPerformanceCurve.SingleOrDefault(clpc =>
            clpc.PerformanceCurveId == curveId
            && clpc.CriterionLibraryId == simulationId);
            Assert.Null(criterionLibraryJoinAfter);
        }

        [Fact]
        public void UpsertOrDeleteScenarioPerformanceCurves_CurveInDbWithEquation_UpdateChangesExpression_ExpressionChangedInDb()
        {
            Setup();
            // Arrange
            var simulationId = Guid.NewGuid();
            var curveId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
            var performanceCurve = ScenarioPerformanceCurveTestSetup.DtoForEntityInDb(TestHelper.UnitOfWork, simulationId, curveId, equation: "2");
            var scenarioCurves = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            var performanceCurveDto = scenarioCurves[0];
            performanceCurveDto.Equation.Expression = "123";

            var updateRows = new List<PerformanceCurveDTO> { performanceCurveDto };

            // Act
            TestHelper.UnitOfWork.PerformanceCurveRepo.UpsertOrDeleteScenarioPerformanceCurvesNonAtomic(updateRows, simulation.Id);

            // Assert
            var scenarioCurvesAfter = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            var performanceCurveDtoAfter = scenarioCurvesAfter[0];
            Assert.Equal("123", performanceCurveDtoAfter.Equation.Expression);
            Assert.Equal(performanceCurveDto.Equation.Id, performanceCurveDtoAfter.Equation.Id);
        }

        [Fact]
        public void UpsertOrDeleteScenarioPerformanceCurves_CurveInDbWithEquation_UpdateRemovesCurve_EquationDeleted()
        {
            Setup();
            // Arrange
            var curveId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var simulationId = simulation.Id;
            var performanceCurve = ScenarioPerformanceCurveTestSetup.DtoForEntityInDb(TestHelper.UnitOfWork, simulationId, curveId, equation: "2");
            var updateRows = new List<PerformanceCurveDTO>();

            // Act
            TestHelper.UnitOfWork.PerformanceCurveRepo.UpsertOrDeleteScenarioPerformanceCurvesNonAtomic(updateRows, simulation.Id);

            // Assert
            var scenarioCurvesAfter = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            Assert.Empty(scenarioCurvesAfter);
        }

        [Fact]
        public void UpsertOrDeleteScenarioPerformanceCurves_EmptySimulationInDb_Adds()
        {
            Setup();
            // Arrange
            var simulationId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
            var attribute = TestHelper.UnitOfWork.AttributeRepo.GetAttributes().First();
            var performanceCurveDto = new PerformanceCurveDTO
            {
                Attribute = attribute.Name,
                Id = Guid.NewGuid(),
                Name = "Curve",
            };
            var updateRows = new List<PerformanceCurveDTO> { performanceCurveDto };

            TestHelper.UnitOfWork.PerformanceCurveRepo.UpsertOrDeleteScenarioPerformanceCurvesNonAtomic(updateRows, simulation.Id);

            var scenarioCurvesAfter = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            Assert.Single(scenarioCurvesAfter);
        }

        [Fact]
        public void UpsertOrDeleteScenarioPerformanceCurves_EmptyLibraryInDb_AddsCurveAndEquationToLibrary()
        {
            Setup();
            // Arrange
            var curveId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var attribute = TestHelper.UnitOfWork.AttributeRepo.GetAttributes().First();
            var equation = new EquationDTO
            {
                Expression = "3",
                Id = Guid.NewGuid(),
            };
            var performanceCurveDto = new PerformanceCurveDTO
            {
                Attribute = attribute.Name,
                Id = Guid.NewGuid(),
                Name = "Curve",
                Equation = equation,
            };
            var updateRows = new List<PerformanceCurveDTO> { performanceCurveDto };

            TestHelper.UnitOfWork.PerformanceCurveRepo.UpsertOrDeleteScenarioPerformanceCurvesNonAtomic(updateRows, simulation.Id);

            var scenarioCurvesAfter = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulation.Id);
            var performanceCurveDtoAfter = scenarioCurvesAfter[0];
            var equationAfter = performanceCurveDtoAfter.Equation;
            Assert.Equal("3", performanceCurveDtoAfter.Equation.Expression);
            var equationEntity = TestHelper.UnitOfWork.Context.Equation.SingleOrDefault(e => e.Id == equationAfter.Id);
            Assert.NotNull(equationEntity);
        }

        [Fact]
        public void UpsertScenarioPerformanceCurveWithEquation_EmptySimulationInDb_AddsCurveAndEquationToSimulation()
        {
            Setup();
            // Arrange
            var curveId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var simulationId = simulation.Id;
            var attribute = TestHelper.UnitOfWork.AttributeRepo.GetAttributes().First();
            var equation = new EquationDTO
            {
                Expression = "3",
                Id = Guid.Empty,
            };
            var performanceCurveDto = new PerformanceCurveDTO
            {
                Attribute = attribute.Name,
                Id = Guid.NewGuid(),
                Name = "Curve",
                Equation = equation,
            };
            var updateRows = new List<PerformanceCurveDTO> { performanceCurveDto };

            TestHelper.UnitOfWork.PerformanceCurveRepo.UpsertOrDeleteScenarioPerformanceCurvesNonAtomic(updateRows, simulation.Id);

            var scenarioCurvesAfter = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulationId);
            var performanceCurveDtoAfter = scenarioCurvesAfter[0];
            var equationAfter = performanceCurveDtoAfter.Equation;
            Assert.Null(equationAfter.Expression);
            var equationEntity = TestHelper.UnitOfWork.Context.Equation.SingleOrDefault(e => e.Id == equationAfter.Id);
            Assert.Null(equationEntity);
        }

        [Fact]
        public void UpsertOrDeleteScenarioPerformanceCurves_CurveInListWithCriterionLibrary_EmptySimulationInDb_AddsPerformanceCurveAndCriterionLibraryToSimulation()
        {
            Setup();
            // Arrange
            var curveId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var attribute = TestHelper.UnitOfWork.AttributeRepo.GetAttributes().First();
            var criterionLibrary = new CriterionLibraryDTO
            {
                Id = Guid.NewGuid(),
                MergedCriteriaExpression = "MergedCriteriaExpression",
                Description = "Description",
                IsSingleUse = true,
            };
            var performanceCurveDto = new PerformanceCurveDTO
            {
                Attribute = attribute.Name,
                Id = Guid.NewGuid(),
                Name = "Curve",
                CriterionLibrary = criterionLibrary,
            };

            var updateRows = new List<PerformanceCurveDTO> { performanceCurveDto };

            TestHelper.UnitOfWork.PerformanceCurveRepo.UpsertOrDeleteScenarioPerformanceCurvesNonAtomic(updateRows, simulation.Id);

            var scenarioCurvesAfter = TestHelper.UnitOfWork.PerformanceCurveRepo.GetScenarioPerformanceCurves(simulation.Id);
            var performanceCurveDtoAfter = scenarioCurvesAfter[0];
            var criterionLibraryAfter = performanceCurveDtoAfter.CriterionLibrary;
            Assert.Equal("MergedCriteriaExpression", criterionLibraryAfter.MergedCriteriaExpression);
        }

        [Fact]
        public void UpsertOrDeleteScenarioPerformanceCurves_CurveHasInvalidAttribute_Throws()
        {
            Setup();
            var libraryId = Guid.NewGuid();
            var libraryDto = PerformanceCurveLibraryTestSetup.TestPerformanceCurveLibraryInDb(TestHelper.UnitOfWork, libraryId);
            var performanceCurveLibraryDto = TestHelper.UnitOfWork.PerformanceCurveRepo.GetPerformanceCurveLibrary(libraryId);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var criterionLibrary = new CriterionLibraryDTO
            {
                Id = Guid.NewGuid(),
                MergedCriteriaExpression = "MergedCriteriaExpression",
                Description = "Description",
                IsSingleUse = true,
            };
            var performanceCurveDto = new PerformanceCurveDTO
            {
                Attribute = "Invalid attribute name",
                Id = Guid.NewGuid(),
                Name = "Curve",
                CriterionLibrary = criterionLibrary,
            };
            var performanceCurves = new List<PerformanceCurveDTO> { performanceCurveDto };

            var exception = Assert.ThrowsAny<Exception>(() =>
            TestHelper.UnitOfWork.PerformanceCurveRepo.UpsertOrDeleteScenarioPerformanceCurvesNonAtomic(performanceCurves, simulation.Id));

            var message = exception.Message;
            Assert.Contains(ErrorMessageConstants.NoAttributeFoundHavingName, message);
        }

        [Fact]
        public void GetPerformanceCurveLibraries_LibraryInDbWithCurve_Gets()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var library = PerformanceCurveLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var curveId = Guid.NewGuid();
            var curve = PerformanceCurveTestSetup.TestLibraryPerformanceCurveInDb(TestHelper.UnitOfWork, library.Id,
                curveId, TestAttributeNames.DeckSeeded);
            library.PerformanceCurves.Add(curve);

            var libraries = TestHelper.UnitOfWork.PerformanceCurveRepo.GetPerformanceCurveLibraries();

            var foundLibrary = libraries.Single(l => l.Id == library.Id);
            ObjectAssertions.Equivalent(library, foundLibrary);
        }

        [Fact]
        public void GetPerformanceCurveLibrariesNoPerformanceCurves_LibraryInDb_Gets()
        {
            var library = PerformanceCurveLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);

            var libraries = TestHelper.UnitOfWork.PerformanceCurveRepo.GetPerformanceCurveLibraries();

            var foundLibrary = libraries.Single(l => l.Id == library.Id);
            ObjectAssertions.Equivalent(library, foundLibrary);
        }
    }
}
