using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.Analysis;
using AppliedResearchAssociates.iAM.DataPersistenceCore;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DataUnitTests.Tests;
using AppliedResearchAssociates.iAM.DataUnitTests.TestUtils;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Attributes;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Repositories;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.SelectableTreatment;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.User;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using MaintainableAsset = AppliedResearchAssociates.iAM.Data.Networking.MaintainableAsset;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests.CommittedProjects
{
    public class CommittedProjectRepoTests
    {
        private UnitOfDataPersistenceWork _testUOW;
        private Mock<IAMContext> _mockedContext;
        private Guid _badScenario = Guid.Parse("0c66674c-8fcb-462b-8765-69d6815e0958");

        public CommittedProjectRepoTests()
        {
            var mockedTestUOW = new Mock<IUnitOfWork>();
            _mockedContext = new Mock<IAMContext>();

            MockedContextBuilder.AddDataSet(_mockedContext, _ => _.Simulation, TestEntitiesForCommittedProjects.Simulations.AsQueryable());
            MockedContextBuilder.AddDataSet(_mockedContext, _ => _.MaintainableAsset, TestEntitiesForCommittedProjects.MaintainableAssetEntities.AsQueryable());
            MockedContextBuilder.AddDataSet(_mockedContext, _ => _.CommittedProject, TestEntitiesForCommittedProjects.CommittedProjectEntities.AsQueryable());
            MockedContextBuilder.AddDataSet(_mockedContext, _ => _.Attribute, TestEntitiesForCommittedProjects.AttribureEntities.AsQueryable());
            MockedContextBuilder.AddDataSet(_mockedContext, _ => _.InvestmentPlan, TestEntitiesForCommittedProjects.InvestmentPlanEntities().AsQueryable());
            MockedContextBuilder.AddDataSet(_mockedContext, _ => _.ScenarioBudget, TestEntitiesForCommittedProjects.ScenarioBudgetEntities.AsQueryable());
            MockedContextBuilder.AddDataSet(_mockedContext, _ => _.ScenarioSelectableTreatment, TestEntitiesForCommittedProjects.FourYearScenarioNoTreatmentEntities().AsQueryable());

            _testUOW = new UnitOfDataPersistenceWork((new Mock<IConfiguration>()).Object, _mockedContext.Object);
        }

        [Fact]
        public void GetForSimulationHandlesBadScenarioId()
        {
            // Arrange
            var repo = new CommittedProjectRepository(_testUOW);
            var simulationDomain = CommittedProjectRepoTestHelpers.CreateSimulation(_badScenario, _testUOW, false);

            // Act & Assert
            Assert.Throws<RowNotInTableException>(() => repo.GetSimulationCommittedProjects(simulationDomain));
        }

        [Fact]
        public void GetForExportWorksWithCommittedProjects()
        {
            // Arrange
            var repo = new CommittedProjectRepository(_testUOW);

            // Act
            var result = repo.GetCommittedProjectsForExport(TestDataForCommittedProjects.SimulationId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(210000, result.Sum(_ => _.Cost));
            Assert.True(result.First() is SectionCommittedProjectDTO);
            Assert.Equal(2, result.First().LocationKeys.Count);
        }

        [Fact]
        public void GetForExportWorksWithoutCommittedProjects()
        {
            // Arrange
            var repo = new CommittedProjectRepository(_testUOW);

            // Act
            var result = repo.GetCommittedProjectsForExport(TestDataForCommittedProjects.NoCommitSimulationId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetForExportHandlesBadScenarioId()
        {
            // Arrange
            var repo = new CommittedProjectRepository(_testUOW);

            // Act & Assert
            Assert.Throws<RowNotInTableException>(() => repo.GetCommittedProjectsForExport(_badScenario));
        }

        [Fact(Skip = "Unable to run with BulkExtensions")]
        public void UpsertWorksForValidCommittedProjectData()
        {
            // Arrange
            var repo = new CommittedProjectRepository(_testUOW);
            var newProjects = TestDataForCommittedProjects.ValidCommittedProjects;
            newProjects.ForEach(_ => _.SimulationId = TestDataForCommittedProjects.NoCommitSimulationId);

            // Act
            repo.UpsertCommittedProjects(newProjects);
        }

        [Fact]
        public void UpsertHandlesBadSimulationId()
        {
            // Arrange
            var repo = new CommittedProjectRepository(_testUOW);
            var newProjects = TestDataForCommittedProjects.ValidCommittedProjects;
            newProjects.ForEach(_ => _.SimulationId = _badScenario);

            // Act & Assert
            var exception = Assert.Throws<RowNotInTableException>(() => repo.UpsertCommittedProjects(newProjects));
            Assert.Contains("simulation ID", exception.Message);
        }

        [Fact]
        public void UpsertHandlesNonExistingBudgets()
        {
            // Arrange
            var repo = new CommittedProjectRepository(_testUOW);
            var newProjects = TestDataForCommittedProjects.ValidCommittedProjects;
            newProjects.ForEach(_ => _.ScenarioBudgetId = Guid.Parse("0d91f67d-d5f4-4c1b-861c-3a5a24aab100"));

            // Act & Assert
            var exception = Assert.Throws<RowNotInTableException>(() => repo.UpsertCommittedProjects(newProjects));
            Assert.Contains("budget IDs", exception.Message);
        }

        [Fact(Skip = "Unable to run with BulkExtensions")]
        public void UpsertWorksWithNullBudgets()
        {
            // Arrange
            var repo = new CommittedProjectRepository(_testUOW);
            var newProjects = TestDataForCommittedProjects.ValidCommittedProjects;
            newProjects.ForEach(_ => _.ScenarioBudgetId = null);

            // Act
            repo.UpsertCommittedProjects(newProjects);
        }

        [Fact]
        public void DeleteSimulationHandlesSimulationWithNoCommitts()
        {
            // Arrange
            var repo = new CommittedProjectRepository(_testUOW);

            // Act
            repo.DeleteSimulationCommittedProjects(TestDataForCommittedProjects.NoCommitSimulationId);

            // No assert required as long as it works
        }

        [Fact(Skip = "Unable to run with BulkExtensions")]
        public void DeleteSpecificWorksWithValidProject()
        {
            // Arrange
            var repo = new CommittedProjectRepository(_testUOW);
            var projectsToDelete = TestDataForCommittedProjects.ValidCommittedProjects.Select(_ => _.Id).ToList();

            // Act
            repo.DeleteSpecificCommittedProjects(projectsToDelete);
        }

        [Fact]
        public void DeleteSpecificHandlesInvalidProject()
        {
            // Arrange
            var repo = new CommittedProjectRepository(_testUOW);
            var projectsToDelete = new List<Guid>() { Guid.Parse("ba5645ae-4f13-4a9f-94fd-2c03d26de500") };

            // Act & Assert
            // No assert here.  If a specific project does not exist but others do, we do not want to throw an error
        }

        [Fact]
        public void CanGetSectionCommittedProjectsForSimulationWithProjects()
        {
            // Arrange
            var repo = new CommittedProjectRepository(_testUOW);

            // Act
            var result = repo.GetSectionCommittedProjectDTOs(TestDataForCommittedProjects.SimulationId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(210000, result.Sum(_ => _.Cost));
            Assert.Equal(2, result.First().LocationKeys.Count);
        }

        [Fact]
        public void GetSectionCommittedProjectsHandlesSimulationWithoutProjects()
        {
            // Arrange
            var repo = new CommittedProjectRepository(_testUOW);

            // Act
            var result = repo.GetSectionCommittedProjectDTOs(TestDataForCommittedProjects.NoCommitSimulationId);

            // Assert
            Assert.Equal(0, result.Count);
            Assert.IsType<List<SectionCommittedProjectDTO>>(result);
        }

        [Fact]
        public void GetSectionCommittedProjectsHandlesBadSimulations()
        { 
            // Arrange
            var repo = new CommittedProjectRepository(_testUOW);

            // Act & Assert
            Assert.Throws<RowNotInTableException>(() => repo.GetSectionCommittedProjectDTOs(_badScenario));
        }

        [Fact]
        public void GetSimulationIdReturnsValidValue()
        {
            // Arrange
            var repo = new CommittedProjectRepository(_testUOW);

            // Act
            var result = repo.GetSimulationId(TestDataForCommittedProjects.CommittedProjectId1);

            // Assert
            Assert.Equal(TestDataForCommittedProjects.SimulationId, result);
        }

        [Fact]
        public void GetSimulationIdHandlesBadProject()
        {
            // Arrange
            var repo = new CommittedProjectRepository(_testUOW);

            // Act & Assert
            Assert.Throws<RowNotInTableException>(() => repo.GetSimulationId(Guid.Parse("aa84643a-24c0-4722-820c-6a1fed01ccac")));
        }
    }
}
