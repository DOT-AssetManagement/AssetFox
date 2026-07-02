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
    public void DeleteCriteriaWithoutTreatmentsForScenario_CriterionHasTreatment_DoesNotDelete()
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
        var settingsBeforeDelete = repo.GetOverrideSettings(simulationId);
        var settingBeforeDelete = settingsBeforeDelete.Single();

        TestHelper.UnitOfWork.AsTransaction(() =>
        repo.DeleteCriteriaWithoutTreatmentsForScenario(simulationId));

        var settingsAfterDelete = repo.GetOverrideSettings(simulationId);
        var settingAfterDelete = settingsAfterDelete.Single();
        ObjectAssertions.Equivalent(settingBeforeDelete, settingAfterDelete);
    }


    [Fact]
    public void DeleteCriteriaWithoutTreatmentsForScenario_CriterionLacksTreatment_Deletes()
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
}
