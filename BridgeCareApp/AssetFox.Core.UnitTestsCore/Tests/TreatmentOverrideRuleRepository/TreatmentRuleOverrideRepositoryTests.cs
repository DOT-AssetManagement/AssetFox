using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.TestUtils;
using Xunit;

namespace AssetFox.Core.UnitTestsCore.Tests.TreatmentRuleOverride;

public class TreatmentRuleOverrideRepositoryTests
{
    [Fact]
    public void GetRuleOverridesByLibraryId_LibraryExistsWithOverride_GetsOverrides()
    {
        var libraryId = Guid.NewGuid();
        var treatmentLibrary = TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, libraryId);
        var repo = TestHelper.UnitOfWork.TreatmentRuleOverrideRepo;
        var overrideId = Guid.NewGuid();
        var overrideRule = TreatmentRuleOverrideTestSetup.ModelForLibraryEntityInDb(TestHelper.UnitOfWork, libraryId, overrideId);

        var actual = repo.GetRuleOverridesByLibraryId(libraryId);

        var returnedRule = actual.Single();
        ObjectAssertions.Equivalent(overrideRule, returnedRule);
    }

    [Fact]
    public void UpsertRuleOverrides_AddingCriterionToSecondExistingOverride_PersistsAllCriteria()
    {
        var libraryId = Guid.NewGuid();
        TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, libraryId);
        var treatments = Enumerable.Range(1, 4)
            .Select(index => TreatmentTestSetup.ModelForSingleTreatmentOfLibrary(
                Guid.NewGuid(),
                $"Treatment {index}"))
            .ToList();
        TestHelper.UnitOfWork.SelectableTreatmentRepo.UpsertOrDeleteTreatments(treatments, libraryId);
        var firstOverride = new TreatmentRuleOverrideDTO
        {
            Id = Guid.NewGuid(),
            Name = "First Override",
            Criteria = new List<TreatmentRuleOverrideCriterionDTO>
            {
                TreatmentRuleOverrideTestSetup.CriterionDto(treatments[0].Id, "First criterion"),
                TreatmentRuleOverrideTestSetup.CriterionDto(treatments[1].Id, "Second criterion")
            }
        };
        var secondOverride = new TreatmentRuleOverrideDTO
        {
            Id = Guid.NewGuid(),
            Name = "Second Override",
            Criteria = new List<TreatmentRuleOverrideCriterionDTO>
            {
                TreatmentRuleOverrideTestSetup.CriterionDto(treatments[2].Id, "Third criterion")
            }
        };
        TestHelper.UnitOfWork.TreatmentRuleOverrideRepo.UpsertRuleOverrides(
            libraryId,
            new List<TreatmentRuleOverrideDTO> { firstOverride, secondOverride });

        var reloadedOverrides = TestHelper.UnitOfWork.TreatmentRuleOverrideRepo
            .GetRuleOverridesByLibraryId(libraryId);
        reloadedOverrides.Single(ruleOverride => ruleOverride.Id == secondOverride.Id).Criteria.Add(
            TreatmentRuleOverrideTestSetup.CriterionDto(treatments[3].Id, "Fourth criterion"));

        TestHelper.UnitOfWork.TreatmentRuleOverrideRepo.UpsertRuleOverrides(libraryId, reloadedOverrides);

        var persistedOverrides = TestHelper.UnitOfWork.TreatmentRuleOverrideRepo
            .GetRuleOverridesByLibraryId(libraryId);
        Assert.Equal(2, persistedOverrides.Single(ruleOverride => ruleOverride.Id == firstOverride.Id).Criteria.Count);
        Assert.Equal(2, persistedOverrides.Single(ruleOverride => ruleOverride.Id == secondOverride.Id).Criteria.Count);
    }

    [Fact]
    public void UpsertRuleOverrides_EmptyTreatmentIdsWithStaleTreatments_RejectsCriterion()
    {
        var libraryId = Guid.NewGuid();
        TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, libraryId);
        var treatmentId = Guid.NewGuid();
        TreatmentTestSetup.ModelForSingleTreatmentOfLibraryInDb(TestHelper.UnitOfWork, libraryId, treatmentId);
        var ruleOverride = TreatmentRuleOverrideTestSetup.ModelForLibraryEntityInDbWithCriterion(
            TestHelper.UnitOfWork,
            libraryId,
            Guid.NewGuid(),
            treatmentId,
            "Expression");
        var persistedOverride = TestHelper.UnitOfWork.TreatmentRuleOverrideRepo
            .GetRuleOverridesByLibraryId(libraryId)
            .Single(rule => rule.Id == ruleOverride.Id);
        Assert.NotEmpty(persistedOverride.Criteria[0].Treatments);
        persistedOverride.Criteria[0].TreatmentIds = new List<Guid>();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            TestHelper.UnitOfWork.TreatmentRuleOverrideRepo.UpsertRuleOverrides(
                libraryId,
                new List<TreatmentRuleOverrideDTO> { persistedOverride }));

        Assert.Equal("Each treatment override criterion must select at least one treatment.", exception.Message);
    }

    [Fact]
    public void DeleteRuleOverride_OverrideInDb_Deletes()
    {
        var libraryId = Guid.NewGuid();
        var treatmentLibrary = TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, libraryId);
        var repo = TestHelper.UnitOfWork.TreatmentRuleOverrideRepo;
        var overrideId = Guid.NewGuid();
        var overrideRule = TreatmentRuleOverrideTestSetup.ModelForLibraryEntityInDb(TestHelper.UnitOfWork, libraryId, overrideId);
        var rulesBefore = repo.GetRuleOverridesByLibraryId(libraryId);
        Assert.NotEmpty(rulesBefore);

        repo.DeleteRuleOverride(overrideId);

        var rulesAfter = repo.GetRuleOverridesByLibraryId(libraryId);
        Assert.Empty(rulesAfter);
    }

    [Fact]
    public void DeleteRuleOverride_DoesNotExist_DoesNotThrow()
    {
        var repo = TestHelper.UnitOfWork.TreatmentRuleOverrideRepo;
        repo.DeleteRuleOverride(Guid.NewGuid());
    }

    [Fact]
    public void ReplaceScenarioRuleOverridesFromLibrary_Does()
    {
        AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
        NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
        var simulationId = Guid.NewGuid();
        SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
        var libraryId = Guid.NewGuid();
        var treatmentLibrary = TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, libraryId);
        var repo = TestHelper.UnitOfWork.TreatmentRuleOverrideRepo;
        var overrideId = Guid.NewGuid();
        var overrideRule = TreatmentRuleOverrideTestSetup.ModelForLibraryEntityInDb(TestHelper.UnitOfWork, libraryId, overrideId);
        var treatmentId = Guid.NewGuid();
        var scenarioTreatment = TreatmentTestSetup.ModelForSingleTreatmentOfSimulationInDb(TestHelper.UnitOfWork, simulationId, treatmentId);
        var scenarioTreatments = new List<TreatmentDTO> { scenarioTreatment };

        repo.ReplaceScenarioRuleOverridesFromLibrary(simulationId, libraryId, scenarioTreatments);

        var scenarioOverrides = repo.GetRuleOverridesBySimulationId(simulationId);
        Assert.Single(scenarioOverrides);
    }

    [Fact]
    public void ReplaceScenarioRuleOverridesFromLibrary_TargetsScenarioTreatmentIds()
    {
        AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
        NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
        var simulationId = Guid.NewGuid();
        SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
        var libraryId = Guid.NewGuid();
        TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, libraryId);
        var libraryTreatmentId = Guid.NewGuid();
        TreatmentTestSetup.ModelForSingleTreatmentOfLibraryInDb(
            TestHelper.UnitOfWork,
            libraryId,
            libraryTreatmentId);
        TreatmentRuleOverrideTestSetup.ModelForLibraryEntityInDbWithCriterion(
            TestHelper.UnitOfWork,
            libraryId,
            Guid.NewGuid(),
            libraryTreatmentId,
            "Expression");
        var scenarioTreatmentId = Guid.NewGuid();
        var scenarioTreatment = TreatmentTestSetup.ModelForSingleTreatmentOfSimulationInDb(
            TestHelper.UnitOfWork,
            simulationId,
            scenarioTreatmentId,
            sourceTreatmentId: libraryTreatmentId);

        TestHelper.UnitOfWork.TreatmentRuleOverrideRepo.ReplaceScenarioRuleOverridesFromLibrary(
            simulationId,
            libraryId,
            new List<TreatmentDTO> { scenarioTreatment });

        var scenarioOverride = Assert.Single(
            TestHelper.UnitOfWork.TreatmentRuleOverrideRepo.GetRuleOverridesBySimulationId(simulationId));
        var scenarioCriterion = Assert.Single(scenarioOverride.Criteria);
        Assert.Equal(scenarioTreatmentId, Assert.Single(scenarioCriterion.TreatmentIds));
        Assert.Equal(scenarioTreatmentId, Assert.Single(scenarioCriterion.Treatments).Id);
        Assert.DoesNotContain(libraryTreatmentId, scenarioCriterion.TreatmentIds);
    }

    [Fact]
    public void UpsertSettings_Does()
    {
        AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
        NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
        var simulationId = Guid.NewGuid();
        SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
        var ruleOverrideId = Guid.NewGuid();
        var treatmentLibraryId = Guid.NewGuid();
        var treatmentLibrary = TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, treatmentLibraryId);
        var libraryTreatmentId = Guid.NewGuid();
        var libraryTreatment = TreatmentTestSetup.ModelForSingleTreatmentOfLibraryInDb(TestHelper.UnitOfWork, treatmentLibraryId, libraryTreatmentId);
        var scenarioTreatmentId = Guid.NewGuid();
        var scenarioTreatment = TreatmentTestSetup.ModelForSingleTreatmentOfSimulationInDb(TestHelper.UnitOfWork, simulationId, sourceTreatmentId: scenarioTreatmentId);

        var ruleOverride = TreatmentRuleOverrideTestSetup.ModelForLibraryEntityInDb(TestHelper.UnitOfWork, treatmentLibraryId, ruleOverrideId);
        var repo = TestHelper.UnitOfWork.TreatmentRuleOverrideRepo;
        var setting = ScenarioTreatmentRuleOverrideSettingDtos.New(ruleOverrideId, ruleOverride.Name, false);
        var settings = new List<ScenarioTreatmentRuleOverrideSettingDTO> { setting };
        var libraryTreatments = new List<TreatmentDTO> { libraryTreatment };
        repo.ReplaceScenarioRuleOverridesFromLibrary(simulationId, treatmentLibraryId, libraryTreatments);
        var overridesBefore = TestHelper.UnitOfWork.TreatmentRuleOverrideRepo.GetRuleOverridesBySimulationId(simulationId);
        var overrideBefore = overridesBefore.Single();
        Assert.True(overrideBefore.IsEnabled);

        TestHelper.UnitOfWork.TreatmentRuleOverrideRepo.UpsertOverrideSettings(simulationId, settings);

        var overridesAfter = TestHelper.UnitOfWork.TreatmentRuleOverrideRepo.GetRuleOverridesBySimulationId(simulationId);
        var overrideAfter = overridesAfter.Single();
        ObjectAssertions.EquivalentExcluding(overrideBefore, overrideAfter, o => o.IsEnabled, o => o.IsEnabledByDefault);
        Assert.False(overrideAfter.IsEnabled);
        Assert.False(overrideAfter.IsEnabledByDefault);
    }

    [Fact]
    public void DeleteScenarioRuleOverrides_Does()
    {
        AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
        NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
        var simulationId = Guid.NewGuid();
        SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
        var libraryId = Guid.NewGuid();
        var treatmentLibrary = TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, libraryId);
        var repo = TestHelper.UnitOfWork.TreatmentRuleOverrideRepo;
        var overrideId = Guid.NewGuid();
        var overrideRule = TreatmentRuleOverrideTestSetup.ModelForLibraryEntityInDb(TestHelper.UnitOfWork, libraryId, overrideId);
        var treatmentId = Guid.NewGuid();
        var scenarioTreatment = TreatmentTestSetup.ModelForSingleTreatmentOfSimulationInDb(TestHelper.UnitOfWork, simulationId, treatmentId);
        var scenarioTreatments = new List<TreatmentDTO> { scenarioTreatment };
        repo.ReplaceScenarioRuleOverridesFromLibrary(simulationId, libraryId, scenarioTreatments);
        repo.DeleteScenarioRuleOverrides(simulationId);
        var scenarioOverrides = repo.GetRuleOverridesBySimulationId(simulationId);
        Assert.Empty(scenarioOverrides);
    }

    [Fact]
    public void GetOverrideSettings_SettingsInDb_Gets()
    {
        AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
        NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
        var simulationId = Guid.NewGuid();
        SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
        var ruleOverrideId = Guid.NewGuid();
        var treatmentLibraryId = Guid.NewGuid();
        var treatmentLibrary = TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, treatmentLibraryId);
        var libraryTreatmentId = Guid.NewGuid();
        var libraryTreatment = TreatmentTestSetup.ModelForSingleTreatmentOfLibraryInDb(TestHelper.UnitOfWork, treatmentLibraryId, libraryTreatmentId);
        var scenarioTreatmentId = Guid.NewGuid();
        var scenarioTreatment = TreatmentTestSetup.ModelForSingleTreatmentOfSimulationInDb(TestHelper.UnitOfWork, simulationId, sourceTreatmentId: scenarioTreatmentId);

        var ruleOverride = TreatmentRuleOverrideTestSetup.ModelForLibraryEntityInDb(TestHelper.UnitOfWork, treatmentLibraryId, ruleOverrideId);
        var repo = TestHelper.UnitOfWork.TreatmentRuleOverrideRepo;
        var setting = ScenarioTreatmentRuleOverrideSettingDtos.New(ruleOverrideId, ruleOverride.Name, false);
        var settings = new List<ScenarioTreatmentRuleOverrideSettingDTO> { setting };
        var libraryTreatments = new List<TreatmentDTO> { libraryTreatment };
        var settingsBefore = repo.GetOverrideSettings(simulationId);
        Assert.Empty(settingsBefore);

        repo.ReplaceScenarioRuleOverridesFromLibrary(simulationId, treatmentLibraryId, libraryTreatments);

        var settingsAfter = repo.GetOverrideSettings(simulationId);
        var settingAfter = settingsAfter.Single();
    }

    [Fact]
    public void DeleteCriteriaWithoutTreatmentsForScenario_CalledViaDeleteScenarioSelectableTreatment_Deletes()
    {
        AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
        NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
        var simulationId = Guid.NewGuid();
        SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
        var ruleOverrideId = Guid.NewGuid();
        var treatmentLibraryId = Guid.NewGuid();
        var treatmentLibrary = TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, treatmentLibraryId);
        var libraryTreatmentId = Guid.NewGuid();
        var libraryTreatment = TreatmentTestSetup.ModelForSingleTreatmentOfLibraryInDb(TestHelper.UnitOfWork, treatmentLibraryId, libraryTreatmentId);
        var scenarioTreatmentId = Guid.NewGuid();
        var scenarioTreatment = TreatmentTestSetup.ModelForSingleTreatmentOfSimulationInDb(TestHelper.UnitOfWork, simulationId, sourceTreatmentId: libraryTreatmentId);
        var ruleOverride = TreatmentRuleOverrideTestSetup.ModelForLibraryEntityInDbWithCriterion(TestHelper.UnitOfWork, treatmentLibraryId, ruleOverrideId, libraryTreatmentId, "Expression");
        var repo = TestHelper.UnitOfWork.TreatmentRuleOverrideRepo;
        var setting = ScenarioTreatmentRuleOverrideSettingDtos.New(ruleOverrideId, ruleOverride.Name, false);
        var settings = new List<ScenarioTreatmentRuleOverrideSettingDTO> { setting };
        var scenarioTreatments = new List<TreatmentDTO> { scenarioTreatment };
        repo.ReplaceScenarioRuleOverridesFromLibrary(simulationId, treatmentLibraryId, scenarioTreatments);
        var treatmentsBeforeDelete = TestHelper.UnitOfWork.SelectableTreatmentRepo.GetScenarioSelectableTreatments(simulationId);
        Assert.Single(treatmentsBeforeDelete);

        TestHelper.UnitOfWork.SelectableTreatmentRepo.DeleteScenarioSelectableTreatment(scenarioTreatment, simulationId);

        var treatmentsAfterDelete = TestHelper.UnitOfWork.SelectableTreatmentRepo.GetScenarioSelectableTreatments(simulationId);
        Assert.Empty(treatmentsAfterDelete);
    }

    [Fact]
    public void DeleteCriteriaWithoutTreatmentsForLibrary_CalledViaDeleteTreatment_Deletes()
    {
        AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
        NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
        var libraryId = Guid.NewGuid();
        var treatmentLibrary = TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, libraryId);
        var ruleOverrideId = Guid.NewGuid();
        var libraryTreatmentId = Guid.NewGuid();
        var libraryTreatment = TreatmentTestSetup.ModelForSingleTreatmentOfLibraryInDb(TestHelper.UnitOfWork, libraryId, libraryTreatmentId);
        var ruleOverride = TreatmentRuleOverrideTestSetup.ModelForLibraryEntityInDbWithCriterion(TestHelper.UnitOfWork, libraryId, ruleOverrideId, libraryTreatmentId, "Expression");
        var repo = TestHelper.UnitOfWork.TreatmentRuleOverrideRepo;
        var treatmentsBeforeDelete = TestHelper.UnitOfWork.SelectableTreatmentRepo.GetSelectableTreatments(libraryId);
        Assert.Single(treatmentsBeforeDelete);

        TestHelper.UnitOfWork.SelectableTreatmentRepo.DeleteTreatment(libraryTreatment, libraryId);

        var treatmentsAfterDelete = TestHelper.UnitOfWork.SelectableTreatmentRepo.GetSelectableTreatments(libraryId);
        Assert.Empty(treatmentsAfterDelete);
    }
}
