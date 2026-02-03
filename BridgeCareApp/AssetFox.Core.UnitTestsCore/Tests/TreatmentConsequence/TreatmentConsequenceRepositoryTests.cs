using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.Assertions;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.TestUtils;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Xunit;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public class TreatmentConsequenceRepositoryTests
    {
        [Fact]
        public void GetTreatmentConsequencesByLibraryIdAndTreatmentName_Does()
        {
            var attributeName = RandomStrings.WithPrefix("attribute");
            AttributeTestSetup.CreateSingleNumericAttribute(TestHelper.UnitOfWork, null, attributeName);
            var treatmentLibraryId = Guid.NewGuid();
            var treatmentLibrary = TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, treatmentLibraryId);
            var treatmentId = Guid.NewGuid();
            var consequenceId = Guid.NewGuid();
            var treatmentName = RandomStrings.WithPrefix("treatment");
            var treatment = TreatmentTestSetup.ModelForSingleTreatmentOfLibraryInDb(
                TestHelper.UnitOfWork, treatmentLibraryId, treatmentId, treatmentName
                );
            var equationId = Guid.NewGuid();
            var consequence = LibraryTreatmentConsequenceTestSetup.ModelForEntityInDb(
                TestHelper.UnitOfWork, treatmentLibraryId, treatmentId,
                consequenceId, attributeName, equationId);

            var result = TestHelper.UnitOfWork.TreatmentConsequenceRepo.GetTreatmentConsequencesByLibraryIdAndTreatmentName(
                treatmentLibraryId, treatmentName);

            var returnedConsequence = result.Single();
            ObjectAssertions.EquivalentExcluding(consequence, returnedConsequence, x => x.CriterionLibrary, x => x.Equation);
            CriterionLibraryDtoAssertions.AssertValidUpsertResult(consequence.CriterionLibrary, returnedConsequence.CriterionLibrary);
        }

        [Fact]
        public void GetTreatmentConsequencesByLibraryIdAndTreatmentName_DoesNotGetEquation()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var attributeName = TestAttributeNames.CulvDurationN;
            var treatmentLibraryId = Guid.NewGuid();
            var treatmentLibrary = TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, treatmentLibraryId);
            var treatmentId = Guid.NewGuid();
            var consequenceId = Guid.NewGuid();
            var treatmentName = RandomStrings.WithPrefix("treatment");
            var treatment = TreatmentTestSetup.ModelForSingleTreatmentOfLibraryInDb(
                TestHelper.UnitOfWork, treatmentLibraryId, treatmentId, treatmentName
                );
            var consequence = LibraryTreatmentConsequenceTestSetup.ModelForEntityInDb(
                TestHelper.UnitOfWork, treatmentLibraryId, treatmentId,
                consequenceId, attributeName);
            Assert.NotEqual(Guid.Empty, consequence.Equation.Id);

            var result = TestHelper.UnitOfWork.TreatmentConsequenceRepo.GetTreatmentConsequencesByLibraryIdAndTreatmentName(
                treatmentLibraryId, treatmentName);

            var returnedConsequence = result.Single();
            var returnedEquation = returnedConsequence.Equation;
            ObjectAssertions.Equivalent(new EquationDTO(), returnedEquation);
        }

        [Fact]
        public void GetTreatmentConsequencesByTreatmentId_ConsequenceInDbWithTreatment_Gets()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var attributeName = TestAttributeNames.CulvDurationN;
            var treatmentLibraryId = Guid.NewGuid();
            var treatmentLibrary = TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, treatmentLibraryId);
            var treatmentId = Guid.NewGuid();
            var consequenceId = Guid.NewGuid();
            var treatmentName = RandomStrings.WithPrefix("treatment");
            var treatment = TreatmentTestSetup.ModelForSingleTreatmentOfLibraryInDb(
                TestHelper.UnitOfWork, treatmentLibraryId, treatmentId, treatmentName
                );
            var consequence = LibraryTreatmentConsequenceTestSetup.ModelForEntityInDb(
                TestHelper.UnitOfWork, treatmentLibraryId, treatmentId,
                consequenceId, attributeName);

            var actual = TestHelper.UnitOfWork.TreatmentConsequenceRepo.GetTreatmentConsequencesByTreatmentId(treatmentId);

            var actualConsequence = actual.Single();
            ObjectAssertions.EquivalentExcluding(consequence, actualConsequence, c => c.CriterionLibrary, c => c.Equation.Id);
            CriterionLibraryDtoAssertions.AssertValidUpsertResult(consequence.CriterionLibrary, actualConsequence.CriterionLibrary);
        }

        [Fact]
        public void GetScenarioTreatmentConsequencesByTreatmentId_ConsequenceInDbWithTreatment_Gets()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var attributeName = TestAttributeNames.CulvDurationN;
            var simulationId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId, networkId: NetworkTestSetup.NetworkId);
            var consequenceId = Guid.NewGuid();
            var treatmentName = RandomStrings.WithPrefix("treatment");
            var treatment = TreatmentTestSetup.ModelForSingleTreatmentOfSimulationInDb(TestHelper.UnitOfWork, simulationId);
            var treatmentId = treatment.Id;
            var consequence = ScenarioTreatmentConsequenceTestSetup.ModelForEntityInDb(
                TestHelper.UnitOfWork, simulationId, treatmentId,
                consequenceId, attributeName);

            var actual = TestHelper.UnitOfWork.TreatmentConsequenceRepo.GetScenarioTreatmentConsequencesByTreatmentId(treatmentId);

            var actualConsequence = actual.Single();
            ObjectAssertions.EquivalentExcluding(consequence, actualConsequence, c => c.CriterionLibrary, c => c.Equation);
            CriterionLibraryDtoAssertions.AssertValidUpsertResult(consequence.CriterionLibrary, actualConsequence.CriterionLibrary);
        }

        [Fact]
        public void UpsertOrDeleteScenarioTreatmentConsequences_ConsequenceDoesNotExist_Adds()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var attributeName = TestAttributeNames.CulvDurationN;
            var simulationId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId, networkId: NetworkTestSetup.NetworkId);
            var consequenceId = Guid.NewGuid();
            var treatmentName = RandomStrings.WithPrefix("treatment");
            var treatment = TreatmentTestSetup.ModelForSingleTreatmentOfSimulationInDb(TestHelper.UnitOfWork, simulationId);
            var treatmentId = treatment.Id;
            var consequence = TreatmentConsequenceDtos.Dto(consequenceId, attributeName);
            var consequenceList = new List<TreatmentConsequenceDTO> { consequence };
            var dictionary = new Dictionary<Guid, List<TreatmentConsequenceDTO>> { { treatmentId, consequenceList } };

            TestHelper.UnitOfWork.TreatmentConsequenceRepo.UpsertOrDeleteScenarioTreatmentConsequences(dictionary, simulationId);

            var treatmentAfter = TestHelper.UnitOfWork.SelectableTreatmentRepo.GetScenarioSelectableTreatmentById(treatmentId);
            var consequencesAfter = treatmentAfter.Treatment.Consequences;
            var consequenceAfter = consequencesAfter.Single();
            ObjectAssertions.EquivalentExcluding(consequence, consequenceAfter, c => c.CriterionLibrary);
        }

        [Fact]
        public void UpsertOrDeleteScenarioTreatmentConsequences_ConsequenceNotInList_Deletes()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var attributeName = TestAttributeNames.CulvDurationN;
            var simulationId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId, networkId: NetworkTestSetup.NetworkId);
            var consequenceId = Guid.NewGuid();
            var treatmentName = RandomStrings.WithPrefix("treatment");
            var treatment = TreatmentTestSetup.ModelForSingleTreatmentOfSimulationInDb(TestHelper.UnitOfWork, simulationId);
            var treatmentId = treatment.Id;
            var consequence = ScenarioTreatmentConsequenceTestSetup.ModelForEntityInDb(
                TestHelper.UnitOfWork, simulationId, treatmentId,
                consequenceId, attributeName);
            var treatmentBefore = TestHelper.UnitOfWork.SelectableTreatmentRepo.GetScenarioSelectableTreatmentById(treatmentId);
            var consequencesBefore = treatmentBefore.Treatment.Consequences;
            var consequenceBefore = consequencesBefore.Single();
            ObjectAssertions.EquivalentExcluding(consequence, consequenceBefore, c => c.CriterionLibrary);
            var emptyConsequenceList = new List<TreatmentConsequenceDTO> { };
            var dictionary = new Dictionary<Guid, List<TreatmentConsequenceDTO>> { { treatmentId, emptyConsequenceList } };

            TestHelper.UnitOfWork.TreatmentConsequenceRepo.UpsertOrDeleteScenarioTreatmentConsequences(dictionary, simulationId);

            var treatmentAfter = TestHelper.UnitOfWork.SelectableTreatmentRepo.GetScenarioSelectableTreatmentById(treatmentId);
            var consequencesAfter = treatmentAfter.Treatment.Consequences;
            Assert.Empty(consequencesAfter);
        }

        [Fact]
        public void UpsertOrDeleteScenarioTreatmentConsequences_ConsequenceExists_Updates()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var attributeName = TestAttributeNames.CulvDurationN;
            var simulationId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId, networkId: NetworkTestSetup.NetworkId);
            var consequenceId = Guid.NewGuid();
            var treatmentName = RandomStrings.WithPrefix("treatment");
            var treatment = TreatmentTestSetup.ModelForSingleTreatmentOfSimulationInDb(TestHelper.UnitOfWork, simulationId);
            var treatmentId = treatment.Id;
            var consequence = ScenarioTreatmentConsequenceTestSetup.ModelForEntityInDb(
                TestHelper.UnitOfWork, simulationId, treatmentId,
                consequenceId, attributeName);
            var treatmentBefore = TestHelper.UnitOfWork.SelectableTreatmentRepo.GetScenarioSelectableTreatmentById(treatmentId);
            var consequencesBefore = treatmentBefore.Treatment.Consequences;
            var consequenceToUpdate = consequencesBefore.Single();
            ObjectAssertions.EquivalentExcluding(consequence, consequenceToUpdate, c => c.CriterionLibrary);
            consequenceToUpdate.ChangeValue = "1423";
            var updateConsequenceList = new List<TreatmentConsequenceDTO> { consequenceToUpdate };
            var dictionary = new Dictionary<Guid, List<TreatmentConsequenceDTO>> { { treatmentId, updateConsequenceList } };

            TestHelper.UnitOfWork.TreatmentConsequenceRepo.UpsertOrDeleteScenarioTreatmentConsequences(dictionary, simulationId);

            var treatmentAfter = TestHelper.UnitOfWork.SelectableTreatmentRepo.GetScenarioSelectableTreatmentById(treatmentId);
            var consequencesAfter = treatmentAfter.Treatment.Consequences;
            var consequenceAfter = consequencesAfter.Single();
            ObjectAssertions.Equivalent(consequenceToUpdate, consequenceAfter);
        }

        [Fact]
        public void UpsertOrDeleteTreatmentConsequences_ConsequenceDoesNotExist_Adds()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var attributeName = TestAttributeNames.CulvDurationN;
            var libraryId = Guid.NewGuid();
            var library = TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, libraryId);
            var consequenceId = Guid.NewGuid();
            var treatmentName = RandomStrings.WithPrefix("treatment");
            var treatment = TreatmentTestSetup.ModelForSingleTreatmentOfLibraryInDb(TestHelper.UnitOfWork, libraryId);
            var consequence = TreatmentConsequenceDtos.Dto(consequenceId, attributeName);
            var consequenceList = new List<TreatmentConsequenceDTO> { consequence };
            var dictionary = new Dictionary<Guid, List<TreatmentConsequenceDTO>> { { treatment.Id, consequenceList } };

            TestHelper.UnitOfWork.TreatmentConsequenceRepo.UpsertOrDeleteTreatmentConsequences(dictionary, libraryId);

            var libraryAfter = TestHelper.UnitOfWork.SelectableTreatmentRepo.GetSingleTreatmentLibary(libraryId);
            var consequencesAfter = libraryAfter.Treatments[0].Consequences;
            var consequenceAfter = consequencesAfter.Single();
            ObjectAssertions.EquivalentExcluding(consequence, consequenceAfter, c => c.CriterionLibrary);
        }

        [Fact]
        public void UpsertOrDeleteTreatmentConsequences_ConsequenceNotInList_Deletes()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var libraryId = Guid.NewGuid();
            var library = TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, libraryId);
            var treatment = TreatmentTestSetup.ModelForSingleTreatmentOfLibraryInDb(TestHelper.UnitOfWork, libraryId);
            var consequenceId = Guid.NewGuid();
            var treatmentName = RandomStrings.WithPrefix("treatment");
            LibraryTreatmentConsequenceTestSetup.ModelForEntityInDb(
                TestHelper.UnitOfWork, libraryId, treatment.Id,
                consequenceId, TestAttributeNames.CulvDurationN);
            var consequencesBefore = TestHelper.UnitOfWork.TreatmentConsequenceRepo.GetTreatmentConsequencesByTreatmentId(treatment.Id);
            Assert.NotEmpty(consequencesBefore);
            var emptyConsequenceList = new List<TreatmentConsequenceDTO> {  };
            var dictionary = new Dictionary<Guid, List<TreatmentConsequenceDTO>> { { treatment.Id, emptyConsequenceList } };

            TestHelper.UnitOfWork.TreatmentConsequenceRepo.UpsertOrDeleteTreatmentConsequences(dictionary, libraryId);

            var libraryAfter = TestHelper.UnitOfWork.SelectableTreatmentRepo.GetSingleTreatmentLibary(libraryId);
            var consequencesAfter = libraryAfter.Treatments[0].Consequences;
            Assert.Empty(consequencesAfter);
        }

        [Fact]
        public void UpsertOrDeleteTreatmentConsequences_ConsequenceExists_Updates()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var libraryId = Guid.NewGuid();
            var library = TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, libraryId);
            var consequenceId = Guid.NewGuid();
            var treatmentName = RandomStrings.WithPrefix("treatment");
            var treatment = TreatmentTestSetup.ModelForSingleTreatmentOfLibraryInDb(TestHelper.UnitOfWork, libraryId);
            LibraryTreatmentConsequenceTestSetup.ModelForEntityInDb(
                TestHelper.UnitOfWork, libraryId, treatment.Id,
                consequenceId, TestAttributeNames.CulvDurationN);
            var treatmentId = treatment.Id;
            var libraryBefore = TestHelper.UnitOfWork.SelectableTreatmentRepo.GetSingleTreatmentLibary(libraryId);
            var treatmentBefore = libraryBefore.Treatments.Single();
            var consequencesBefore = treatmentBefore.Consequences;
            var consequenceToUpdate = consequencesBefore.Single();
            consequenceToUpdate.ChangeValue = "1423";
            var updateConsequenceList = new List<TreatmentConsequenceDTO> { consequenceToUpdate };
            var dictionary = new Dictionary<Guid, List<TreatmentConsequenceDTO>> { { treatmentId, updateConsequenceList } };

            TestHelper.UnitOfWork.TreatmentConsequenceRepo.UpsertOrDeleteTreatmentConsequences(dictionary, libraryId);

            var libraryAfter = TestHelper.UnitOfWork.SelectableTreatmentRepo.GetSingleTreatmentLibary(libraryId);
            var consequencesAfter = libraryAfter.Treatments.Single().Consequences;
            var consequenceAfter = consequencesAfter.Single();
            ObjectAssertions.EquivalentExcluding(consequenceToUpdate, consequenceAfter,
                c => c.Equation.Id, c => c.CriterionLibrary.Id);
        }
    }
}
