using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AssetFox.Core.Analysis;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers;
using AssetFox.Core.UnitTestsCore.TestUtils;
using Xunit;
using AnalysisSelectableTreatment = AssetFox.Core.Analysis.SelectableTreatment;

namespace AssetFox.Core.UnitTestsCore.Tests.CommittedProjects
{
    public class CommittedProjectMapperTests
    {
        private Simulation testSimulation;
        private SimulationEntity simulationSource;

        public CommittedProjectMapperTests()
        {
            simulationSource = TestEntitiesForCommittedProjects.Simulations.Single(_ => _.Name == "FourYearTest");
            var debugExplorer = new Explorer("dummy");
            var dictionary = new Dictionary<Guid, string>();
            var testNetwork = simulationSource.Network.ToDomain(debugExplorer, dictionary);
            simulationSource.CreateSimulation(testNetwork, DateTime.Now, DateTime.Now);
            testSimulation = testNetwork.Simulations.First();
            simulationSource.InvestmentPlan.FillSimulationInvestmentPlan(testSimulation);
        }

        [Fact]
        public void MapperAddsNoTreatmentWhenFlagEnabled()
        {
            // Arrange
            var providedCommittedProject = simulationSource.CommittedProjects.First(_ => _.Year == 2025);
            var testAssetId = simulationSource.Network.MaintainableAssets.First().Id;
            var noTreatment = TestEntitiesForCommittedProjects.FourYearScenarioNoTreatment();
            AnalysisSelectableTreatment myTreatment = new AnalysisSelectableTreatment(testSimulation);
            IReadOnlyCollection<AnalysisSelectableTreatment> selectableTreatments = new List<AnalysisSelectableTreatment> { myTreatment };


            // Act
            providedCommittedProject.CreateCommittedProject(
                testSimulation,
                selectableTreatments,
                testAssetId,
                true,
                0,
                noTreatment);

            // Assert
            var providedName = providedCommittedProject.Name.Single();
            Assert.Equal(4, testSimulation.CommittedProjects.Count);
            Assert.Single(testSimulation.CommittedProjects.Where(_ => _.Name == providedName));
            Assert.Equal(3, testSimulation.CommittedProjects.Where(_ => _.Name == noTreatment.Name).Count());
        }

        [Fact]
        public void MapperDoesNotAddNoTreatmentWhenFlagDisabled()
        {
            // Arrange
            var providedCommittedProject = simulationSource.CommittedProjects.First();
            var testAssetId = simulationSource.Network.MaintainableAssets.First().Id;
            AnalysisSelectableTreatment myTreatment = new AnalysisSelectableTreatment(testSimulation);
            IReadOnlyCollection<AnalysisSelectableTreatment> selectableTreatments = new List<AnalysisSelectableTreatment> { myTreatment };


            // Act
            providedCommittedProject.CreateCommittedProject(
                testSimulation,
                selectableTreatments,
                testAssetId,
                false,
                0,
                TestEntitiesForCommittedProjects.FourYearScenarioNoTreatment());

            // Assert
            Assert.Equal(1, testSimulation.CommittedProjects.Count);
        }

        [Fact]
        public void MapperHandlesAssetWithMultipleProjectsInDifferentYears()
        {
            // Arrange
            var firstCommittedProject = simulationSource.CommittedProjects.First();
            var secondCommittedProject = simulationSource.CommittedProjects.Last();
            secondCommittedProject.Name = ["Something Else"];
            var testAssetId = simulationSource.Network.MaintainableAssets.First().Id;
            var noTreatment = TestEntitiesForCommittedProjects.FourYearScenarioNoTreatment();
            AnalysisSelectableTreatment myTreatment = new AnalysisSelectableTreatment(testSimulation);
            IReadOnlyCollection<AnalysisSelectableTreatment> selectableTreatments = new List<AnalysisSelectableTreatment> { myTreatment };


            // Act
            firstCommittedProject.CreateCommittedProject(testSimulation, selectableTreatments, testAssetId, true, 0, noTreatment);
            secondCommittedProject.CreateCommittedProject(testSimulation, selectableTreatments, testAssetId, true, 0, noTreatment);

            // Assert
            var firstName = firstCommittedProject.Name.Single();
            var secondName = secondCommittedProject.Name.Single();
            Assert.Equal(4, testSimulation.CommittedProjects.Count);
            Assert.Single(testSimulation.CommittedProjects.Where(_ => _.Name == firstName));
            Assert.Single(testSimulation.CommittedProjects.Where(_ => _.Name == secondName));
            Assert.Equal(2, testSimulation.CommittedProjects.Where(_ => _.Name == noTreatment.Name).Count());
        }

        [Fact]
        public void MapperDoesNotComplainAboutAssetWithMultipleProjectsInSameYear()
        {
            // The mapper should go ahead and map bad configurations such as two different
            // committed projects on the same asset in the same year. We don't want to do that,
            // but the mapper is the wrong place to enforce it. This checks that the mapper does not
            // enforce it.
            // Arrange
            var firstCommittedProject = simulationSource.CommittedProjects.First(_ => _.Year == 2025);
            var secondCommittedProject = simulationSource.CommittedProjects.First(_ => _.Year != 2025);
            secondCommittedProject.Name = ["Something Else"];
            secondCommittedProject.Year = firstCommittedProject.Year;
            var testAssetId = simulationSource.Network.MaintainableAssets.First().Id;
            var noTreatment = TestEntitiesForCommittedProjects.FourYearScenarioNoTreatment();

            AnalysisSelectableTreatment myTreatment = new AnalysisSelectableTreatment(testSimulation);
            IReadOnlyCollection<AnalysisSelectableTreatment> selectableTreatments = new List<AnalysisSelectableTreatment> { myTreatment };

            // Act
            firstCommittedProject.CreateCommittedProject(testSimulation, selectableTreatments, testAssetId, true, 0, noTreatment);
            secondCommittedProject.CreateCommittedProject(testSimulation, selectableTreatments, testAssetId, true, 0, noTreatment);

            // Assert
            var secondName = secondCommittedProject.Name.Single();
            Assert.Equal(5, testSimulation.CommittedProjects.Count);
            Assert.Single(testSimulation.CommittedProjects.Where(_ => _.Name == secondName));
            Assert.Equal(3, testSimulation.CommittedProjects.Where(_ => _.Name == noTreatment.Name).Count());
        }
    }
}
