using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.Data.Mappers;
using AppliedResearchAssociates.iAM.DataPersistenceCore;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers;
using AppliedResearchAssociates.iAM.DataUnitTests;
using AppliedResearchAssociates.iAM.DataUnitTests.Tests;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Attributes;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Repositories;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.SelectableTreatment;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.User;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using Xunit;
using MaintainableAsset = AppliedResearchAssociates.iAM.Data.Networking.MaintainableAsset;


namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests.CommittedProjects
{
    public class CommittedProjectRepositoryRealDatabaseTests
    {

        [Fact]
        public async Task DeleteSpecificWorksWithValidProject()
        {
            // Arrange
            TestHelper.UnitOfWork.ClearAttributeIdNameCache();
            var repo = new CommittedProjectRepository(TestHelper.UnitOfWork);

            // Set up a network with maintainable assets
            Guid networkId = Guid.NewGuid();
            var maintainableAssets = new List<MaintainableAsset>();
            var assetId = Guid.NewGuid();
            var locationIdentifier = RandomStrings.WithPrefix("Location");
            var location = Locations.Section(locationIdentifier);
            var maintainableAsset = new MaintainableAsset(assetId, networkId, location, "[Deck_Area]");
            var maintainableAssetEntity = maintainableAsset.ToEntity(networkId);
            var maintainableAssetLocation = new MaintainableAssetLocationEntity()
            {
                Id = Guid.NewGuid(),
                LocationIdentifier = "353",
                Discriminator = DataPersistenceConstants.SectionLocation,
            };
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            maintainableAssetEntity.MaintainableAssetLocation = maintainableAssetLocation;
            var testMaintainableAsset = maintainableAssetEntity.ToDomain(locationIdentifier);
            maintainableAssets.Add(testMaintainableAsset);
            var network = NetworkTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, maintainableAssets, networkId, TestAttributeIds.CulvDurationNId);
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            // Setup a simulation based on network
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, Guid.Parse("dcdacfde-02da-4109-b8aa-add932756dee"), "Test Simulation", user.Id, networkId);
            simulation.NetworkId = network.Id;
            var ip = InvestmentPlanTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulation.Id, null, 2023, 2);
            // Set up a selectable treatment for the test with sample budgets
            var treatmentbudget = TreatmentBudgetDtos.Dto();
            var libraryId = Guid.NewGuid();
            var treatmentId = Guid.NewGuid();
            var treatment = TreatmentDtos.DtoWithEmptyCostsAndConsequencesLists(treatmentId);
            var costId = Guid.NewGuid();
            var costLibraryId = Guid.NewGuid();
            var insertCostEquationId = Guid.NewGuid();
            var cost = TreatmentCostDtos.WithEquationAndCriterionLibrary(costId, insertCostEquationId, costLibraryId, "equation", "mergedCriteriaExpression");
            treatment.Costs.Add(cost);
            treatment.Budgets = new List<TreatmentBudgetDTO>() { treatmentbudget };
            treatment.BudgetIds = new List<Guid> { };
            var treatments = new List<TreatmentDTO> { treatment };
            TestHelper.UnitOfWork.SelectableTreatmentRepo.UpsertOrDeleteScenarioSelectableTreatment(treatments, simulation.Id);

            // Set up committed projects for the test
            var committedProjectId1 = Guid.NewGuid();
            var committedProjectId2 = Guid.NewGuid();
            List<SectionCommittedProjectDTO> sectionCommittedProjects = CreateTestCommittedProjects(simulation.Id, committedProjectId1, committedProjectId2);
            var budgetName = RandomStrings.WithPrefix("Budget");
            var budgetId = Guid.NewGuid();
            var budgetDto = BudgetDtos.New(budgetId, budgetName);
            var budgetDtos = new List<BudgetDTO> { budgetDto };
            ScenarioBudgetTestSetup.UpsertOrDeleteScenarioBudgets(TestHelper.UnitOfWork, budgetDtos, simulation.Id);
            sectionCommittedProjects.ForEach(_ => _.ScenarioBudgetId = budgetId);
            TestHelper.UnitOfWork.CommittedProjectRepo.UpsertCommittedProjects(sectionCommittedProjects);
            var committedProjectsBefore = TestHelper.UnitOfWork.CommittedProjectRepo.GetSectionCommittedProjectDTOs(simulation.Id);
            var committedProjectId = committedProjectsBefore[0].Id;
            var projectIds = new List<Guid> { committedProjectId };

            TestHelper.UnitOfWork.CommittedProjectRepo.DeleteSpecificCommittedProjects(projectIds);

            var committedProjectsAfter = TestHelper.UnitOfWork.CommittedProjectRepo.GetSectionCommittedProjectDTOs(simulation.Id);
            Assert.Equal(committedProjectsBefore.Count - 1, committedProjectsAfter.Count);
            ObjectAssertions.Equivalent(committedProjectsBefore[1], committedProjectsAfter[0]);
        }

        [Fact]
        public async Task UpsertWorksForValidCommittedProjects()
        {
            // Arrange
            var repo = new CommittedProjectRepository(TestHelper.UnitOfWork);

            // Set up a network with maintainable assets
            Guid networkId = Guid.NewGuid();
            var maintainableAssets = new List<MaintainableAsset>();
            var assetId = Guid.NewGuid();
            var locationIdentifier = RandomStrings.WithPrefix("Location");
            var location = Locations.Section(locationIdentifier);
            var maintainableAsset = new MaintainableAsset(assetId, networkId, location, "[Deck_Area]");
            var maintainableAssetEntity = maintainableAsset.ToEntity(networkId);
            var maintainableAssetLocation = new MaintainableAssetLocationEntity()
            {
                Id = Guid.NewGuid(),
                LocationIdentifier = "3",
                Discriminator = DataPersistenceConstants.SectionLocation,
            };
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            maintainableAssetEntity.MaintainableAssetLocation = maintainableAssetLocation;
            var testMaintainableAsset = maintainableAssetEntity.ToDomain(locationIdentifier);
            maintainableAssets.Add(testMaintainableAsset);
            var network = NetworkTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, maintainableAssets, networkId, TestAttributeIds.CulvDurationNId);
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            // Setup a simulation based on network
            var simulationId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId, "Test Simulation", user.Id, networkId);
            simulation.NetworkId = network.Id;
            var ip = InvestmentPlanTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulation.Id, null, 2023, 2);
            // Set up a selectable treatment for the test with sample budgets
            var treatmentbudget = TreatmentBudgetDtos.Dto();
            var libraryId = Guid.NewGuid();
            var treatmentId = Guid.NewGuid();
            var treatment = TreatmentDtos.DtoWithEmptyCostsAndConsequencesLists(treatmentId);
            var costId = Guid.NewGuid();
            var costLibraryId = Guid.NewGuid();
            var insertCostEquationId = Guid.NewGuid();
            var cost = TreatmentCostDtos.WithEquationAndCriterionLibrary(costId, insertCostEquationId, costLibraryId, "equation", "mergedCriteriaExpression");
            treatment.Costs.Add(cost);
            treatment.Budgets = new List<TreatmentBudgetDTO>() { treatmentbudget };
            treatment.BudgetIds = new List<Guid> { };
            var treatments = new List<TreatmentDTO> { treatment };
            TestHelper.UnitOfWork.SelectableTreatmentRepo.UpsertOrDeleteScenarioSelectableTreatment(treatments, simulation.Id);

            // Set up committed projects for the test
            var committedProjectId1 = Guid.NewGuid();
            var committedProjectId2 = Guid.NewGuid();
            List<SectionCommittedProjectDTO> sectionCommittedProjects = CreateTestCommittedProjects(simulation.Id, committedProjectId1, committedProjectId2);

            sectionCommittedProjects.ForEach(_ => _.ScenarioBudgetId = null);
            
            var exception = Assert.Throws<Exception>(() => 
            TestHelper.UnitOfWork.CommittedProjectRepo.UpsertCommittedProjects(sectionCommittedProjects));
        }

        [Fact]
        public async Task UpsertWorksWithNullBudget()
        {
            // Arrange
            TestHelper.UnitOfWork.ClearAttributeIdNameCache();
            var repo = new CommittedProjectRepository(TestHelper.UnitOfWork);

            // Set up a network with maintainable assets
            Guid networkId = Guid.NewGuid();
            var maintainableAssets = new List<MaintainableAsset>();
            var assetId = Guid.NewGuid();
            var locationIdentifier = RandomStrings.WithPrefix("Location");
            var location = Locations.Section(locationIdentifier);
            var maintainableAsset = new MaintainableAsset(assetId, networkId, location, "[Deck_Area]");
            var maintainableAssetEntity = maintainableAsset.ToEntity(networkId);
            var maintainableAssetLocation = new MaintainableAssetLocationEntity()
            {
                Id = Guid.NewGuid(),
                LocationIdentifier = "3",
                Discriminator = DataPersistenceConstants.SectionLocation,
            };
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            maintainableAssetEntity.MaintainableAssetLocation = maintainableAssetLocation;
            var testMaintainableAsset = maintainableAssetEntity.ToDomain(locationIdentifier);
            maintainableAssets.Add(testMaintainableAsset);
            var network = NetworkTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, maintainableAssets, networkId, TestAttributeIds.CulvDurationNId);
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            // Setup a simulation based on network
            var simulationId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId, "Test Simulation", user.Id, networkId);
            simulation.NetworkId = network.Id;
            var ip = InvestmentPlanTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulation.Id, null, 2023, 2);
            // Set up a selectable treatment for the test with sample budgets
            var treatmentbudget = TreatmentBudgetDtos.Dto();
            var libraryId = Guid.NewGuid();
            var treatmentId = Guid.NewGuid();
            var treatment = TreatmentDtos.DtoWithEmptyCostsAndConsequencesLists(treatmentId);
            var costId = Guid.NewGuid();
            var costLibraryId = Guid.NewGuid();
            var insertCostEquationId = Guid.NewGuid();
            var cost = TreatmentCostDtos.WithEquationAndCriterionLibrary(costId, insertCostEquationId, costLibraryId, "equation", "mergedCriteriaExpression");
            treatment.Costs.Add(cost);
            treatment.Budgets = new List<TreatmentBudgetDTO>() { treatmentbudget };
            treatment.BudgetIds = new List<Guid> { };
            var treatments = new List<TreatmentDTO> { treatment };
            TestHelper.UnitOfWork.SelectableTreatmentRepo.UpsertOrDeleteScenarioSelectableTreatment(treatments, simulation.Id);

            // Set up committed projects for the test
            var committedProjectId1 = Guid.NewGuid();
            var committedProjectId2 = Guid.NewGuid();
            List<SectionCommittedProjectDTO> sectionCommittedProjects = CreateTestCommittedProjects(simulation.Id, committedProjectId1, committedProjectId2);
            var budgetName = RandomStrings.WithPrefix("Budget");
            var budgetId = Guid.NewGuid();
            var budgetDto = BudgetDtos.New(budgetId, budgetName);
            var budgetDtos = new List<BudgetDTO> { budgetDto };
            ScenarioBudgetTestSetup.UpsertOrDeleteScenarioBudgets(TestHelper.UnitOfWork, budgetDtos, simulation.Id);

            sectionCommittedProjects.ForEach(_ => _.ScenarioBudgetId = budgetId);
            TestHelper.UnitOfWork.CommittedProjectRepo.UpsertCommittedProjects(sectionCommittedProjects);

        }

        [Fact]
        public async Task GetForSimulationWorksWithCommittedProjects()
        {
            // Arrange
            TestHelper.UnitOfWork.ClearAttributeIdNameCache();
            var repo = new CommittedProjectRepository(TestHelper.UnitOfWork);

            // Set up a network with maintainable assets
            Guid networkId = Guid.NewGuid();
            var maintainableAssets = new List<MaintainableAsset>();
            var assetId = TestDataForCommittedProjects.MaintainableAssetId1;
            var locationIdentifier = RandomStrings.WithPrefix("Location");
            var location = Locations.Section(locationIdentifier);
            var maintainableAsset = new MaintainableAsset(assetId, networkId, location, "[Deck_Area]");
            var maintainableAssetEntity = maintainableAsset.ToEntity(networkId);
            var maintainableAssetLocation = new MaintainableAssetLocationEntity()
            {
                Id = Guid.NewGuid(),
                LocationIdentifier = "3",
                Discriminator = DataPersistenceConstants.SectionLocation,
            };
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            maintainableAssetEntity.MaintainableAssetLocation = maintainableAssetLocation;
            var testMaintainableAsset = maintainableAssetEntity.ToDomain(locationIdentifier);
            maintainableAssets.Add(testMaintainableAsset);
            var network = NetworkTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, maintainableAssets, networkId, TestAttributeIds.CulvDurationNId);
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            // Setup a simulation based on network
            var simulationId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId, "Test Simulation", user.Id, networkId);
            simulation.NetworkId = network.Id;
            var ip = InvestmentPlanTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulation.Id, null, 2023, 2);
            // Set up a selectable treatment for the test with sample budgets
            var treatmentbudget = TreatmentBudgetDtos.Dto();
            var libraryId = Guid.NewGuid();
            var treatmentId = Guid.NewGuid();
            var treatment = TreatmentDtos.DtoWithEmptyCostsAndConsequencesLists(treatmentId);
            var costId = Guid.NewGuid();
            var costLibraryId = Guid.NewGuid();
            var insertCostEquationId = Guid.NewGuid();
            var cost = TreatmentCostDtos.WithEquationAndCriterionLibrary(costId, insertCostEquationId, costLibraryId, "equation", "mergedCriteriaExpression");
            treatment.Costs.Add(cost);
            treatment.Budgets = new List<TreatmentBudgetDTO>() { treatmentbudget };
            treatment.BudgetIds = new List<Guid> { };
            var treatments = new List<TreatmentDTO> { treatment };
            TestHelper.UnitOfWork.SelectableTreatmentRepo.UpsertOrDeleteScenarioSelectableTreatment(treatments, simulation.Id);

            // Set up committed projects for the test
            var committedProjectId1 = Guid.NewGuid();
            var committedProjectId2 = Guid.NewGuid();
            List<SectionCommittedProjectDTO> sectionCommittedProjects = CreateTestCommittedProjects(simulation.Id, committedProjectId1, committedProjectId2);
            var budgetName = RandomStrings.WithPrefix("Budget");
            var budgetId = Guid.NewGuid();
            var budgetDto = BudgetDtos.New(budgetId, budgetName);
            var budgetDtos = new List<BudgetDTO> { budgetDto };
            ScenarioBudgetTestSetup.UpsertOrDeleteScenarioBudgets(TestHelper.UnitOfWork, budgetDtos, simulation.Id);

            sectionCommittedProjects.ForEach(_ => _.ScenarioBudgetId = budgetId);
            TestHelper.UnitOfWork.CommittedProjectRepo.UpsertCommittedProjects(sectionCommittedProjects);
            // Act
            var testSimulation = CommittedProjectRepoTestHelpers.CreateSimulation(simulation.Id, TestHelper.UnitOfWork, true);
            testSimulation.Network.Id = networkId;

            TestHelper.UnitOfWork.CommittedProjectRepo.GetSimulationCommittedProjects(testSimulation);

            // Assert
            Assert.Equal(2, testSimulation.CommittedProjects.Count);
            Assert.Equal(220000, testSimulation.CommittedProjects.Sum(_ => _.Cost));
        }

        [Fact]
        public async Task GetForSimulationWorksWithNoTreatmentBeforeCommittedProjects()
        {
            // Arrange
            TestHelper.UnitOfWork.ClearAttributeIdNameCache();
            var repo = new CommittedProjectRepository(TestHelper.UnitOfWork);

            // Set up a network with maintainable assets
            Guid networkId = Guid.NewGuid();
            var maintainableAssets = new List<MaintainableAsset>();
            var assetId = TestDataForCommittedProjects.MaintainableAssetId3;
            var locationIdentifier = RandomStrings.WithPrefix("Location");
            var location = Locations.Section(locationIdentifier);
            var maintainableAsset = new MaintainableAsset(assetId, networkId, location, "[Deck_Area]");
            var maintainableAssetEntity = maintainableAsset.ToEntity(networkId);
            var deckAreaAttribute = AttributeDtoDomainMapper.ToDomain(AttributeDtos.DeckArea, "");
            var maintainableAssetLocation = new MaintainableAssetLocationEntity()
            {
                Id = Guid.NewGuid(),
                LocationIdentifier = "3",
                Discriminator = DataPersistenceConstants.SectionLocation,
            };
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            maintainableAssetEntity.MaintainableAssetLocation = maintainableAssetLocation;
            var testMaintainableAsset = maintainableAssetEntity.ToDomain(locationIdentifier);
            maintainableAssets.Add(testMaintainableAsset);
            var network = NetworkTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, maintainableAssets, networkId, TestAttributeIds.CulvDurationNId);
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            // Setup a simulation based on network
            var simulationId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId, "Test Simulation", user.Id, networkId);
            TestHelper.UnitOfWork.SimulationRepo.SetNoTreatmentBeforeCommitted(simulationId);
            simulation.NetworkId = network.Id;
            var ip = InvestmentPlanTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulation.Id, null, 2022, 3);
            // Set up a selectable treatment for the test with sample budgets
            var treatmentbudget = TreatmentBudgetDtos.Dto();
            var libraryId = Guid.NewGuid();
            var treatmentId = Guid.NewGuid();
            var treatment = TreatmentDtos.DtoWithEmptyCostsAndConsequencesLists(treatmentId);
            var costId = Guid.NewGuid();
            var costLibraryId = Guid.NewGuid();
            var insertCostEquationId = Guid.NewGuid();
            var cost = TreatmentCostDtos.WithEquationAndCriterionLibrary(costId, insertCostEquationId, costLibraryId, "[DECK_AREA]", "mergedCriteriaExpression");
            treatment.Costs.Add(cost);
            treatment.Budgets = new List<TreatmentBudgetDTO>() { treatmentbudget };
            treatment.BudgetIds = new List<Guid> { };
            var treatments = new List<TreatmentDTO> { treatment };
            TestHelper.UnitOfWork.SelectableTreatmentRepo.UpsertOrDeleteScenarioSelectableTreatment(treatments, simulation.Id);

            // Set up committed projects for the test
            var committedProjectId1 = Guid.NewGuid();
            var committedProjectId2 = Guid.NewGuid();
            List<SectionCommittedProjectDTO> sectionCommittedProjects = CreateTestCommittedProjects(simulation.Id, committedProjectId1, committedProjectId2);
            var budgetName = RandomStrings.WithPrefix("Budget");
            var budgetId = Guid.NewGuid();
            var budgetDto = BudgetDtos.New(budgetId, budgetName);
            var budgetDtos = new List<BudgetDTO> { budgetDto };
            ScenarioBudgetTestSetup.UpsertOrDeleteScenarioBudgets(TestHelper.UnitOfWork, budgetDtos, simulation.Id);

            sectionCommittedProjects.ForEach(_ => _.ScenarioBudgetId = budgetId);
            TestHelper.UnitOfWork.CommittedProjectRepo.UpsertCommittedProjects(sectionCommittedProjects);
            // Act
            var testSimulation = CommittedProjectRepoTestHelpers.CreateSimulation(simulation.Id, TestHelper.UnitOfWork, true);
            testSimulation.Network.Id = networkId;
            AggregatedResultTestSetup.AddSingleNumericAggregatedResultToDb(
                TestHelper.UnitOfWork,
                maintainableAsset,
                deckAreaAttribute,
                3100);

            TestHelper.UnitOfWork.CommittedProjectRepo.GetSimulationCommittedProjects(testSimulation);

            // Assert
            Assert.Equal(3, testSimulation.CommittedProjects.Count);
            Assert.Equal(220000 + 3100, testSimulation.CommittedProjects.Sum(_ => _.Cost));
        }

        [Fact]
        public async Task GetForSimulationWorksWithoutCommittedProjects()
        {
            // Arrange
            var repo = new CommittedProjectRepository(TestHelper.UnitOfWork);

            // Set up a network with maintainable assets
            Guid networkId = Guid.Parse("119AD446-3330-426B-864D-E9D471949D6B");
            var maintainableAssets = new List<MaintainableAsset>();
            var assetId = TestDataForCommittedProjects.MaintainableAssetId2;
            var locationIdentifier = RandomStrings.WithPrefix("Location");
            var location = Locations.Section(locationIdentifier);
            var maintainableAsset = new MaintainableAsset(assetId, networkId, location, "[Deck_Area]");
            var maintainableAssetEntity = maintainableAsset.ToEntity(networkId);
            var maintainableAssetLocation = new MaintainableAssetLocationEntity()
            {
                Id = Guid.Parse("ffff6f5d-0559-4363-aad0-e13849b8e369"),
                LocationIdentifier = "3",
                Discriminator = DataPersistenceConstants.SectionLocation,
            };
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            maintainableAssetEntity.MaintainableAssetLocation = maintainableAssetLocation;
            var testMaintainableAsset = maintainableAssetEntity.ToDomain(locationIdentifier);
            maintainableAssets.Add(testMaintainableAsset);
            var network = NetworkTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, maintainableAssets, networkId, TestAttributeIds.CulvDurationNId);

            // Setup a simulation based on network
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, TestDataForCommittedProjects.NoCommitSimulationId, "Test Simulation", user.Id, networkId);
            InvestmentPlanTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulation.Id, null, 2023);
            simulation.NetworkId = network.Id;

            // Set up a selectable treatment for the test with sample budgets
            var testBudget = new TreatmentBudgetDTO
            {
                Id = Guid.NewGuid(),
                Name = "Budget Test 1"
            };
            var libraryId = Guid.NewGuid();
            var treatmentId = Guid.NewGuid();
            var treatment = TreatmentDtos.DtoWithEmptyCostsAndConsequencesLists(treatmentId);
            var costId = Guid.NewGuid();
            var costLibraryId = Guid.NewGuid();
            var insertCostEquationId = Guid.NewGuid();
            var cost = TreatmentCostDtos.WithEquationAndCriterionLibrary(costId, insertCostEquationId, costLibraryId, "equation", "mergedCriteriaExpression");
            treatment.Costs.Add(cost);
            treatment.Budgets = new List<TreatmentBudgetDTO>() { testBudget };
            treatment.BudgetIds = new List<Guid> { libraryId, treatmentId };
            var treatments = new List<TreatmentDTO> { treatment };
            TestHelper.UnitOfWork.SelectableTreatmentRepo.UpsertOrDeleteScenarioSelectableTreatment(treatments, simulation.Id);

            // Act
            var testSimulation = CommittedProjectRepoTestHelpers.CreateSimulation(simulation.Id, TestHelper.UnitOfWork, false);
            testSimulation.Network.Id = Guid.Parse("502C1684-C8B6-48FD-9725-A2295AA3E0F0");
            TestHelper.UnitOfWork.CommittedProjectRepo.GetSimulationCommittedProjects(testSimulation);

            // Assert
            Assert.Equal(0, testSimulation.CommittedProjects.Count);
        }

        [Fact]
        public void DeleteSimulationHandlesInvalidSimulation()
        {
            // Arrange
            var repo = new CommittedProjectRepository(TestHelper.UnitOfWork);

            var nonexistentSimulationId = Guid.NewGuid();
            // Act & Assert
            Assert.Throws<RowNotInTableException>(() => repo.DeleteSimulationCommittedProjects(nonexistentSimulationId));
        }

        private List<SectionCommittedProjectDTO> CreateTestCommittedProjects(Guid simulationId, Guid committedProjectId1, Guid committedProjectId2)
        {
            List<SectionCommittedProjectDTO> testCommittedProjects = new List<SectionCommittedProjectDTO>();
            var committedProject = SectionCommittedProjectDtos.Dto1(committedProjectId1, simulationId);
            var committedProject2 = SectionCommittedProjectDtos.Dto2(committedProjectId2, simulationId);
            testCommittedProjects.Add(committedProject);
            testCommittedProjects.Add(committedProject2);
            return testCommittedProjects;
        }
    }
}
