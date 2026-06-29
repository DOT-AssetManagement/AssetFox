using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.TestUtils;
using Xunit;

namespace AssetFox.Core.UnitTestsCore.Tests.TreatmentOverrideRuleRepository;

public class TreatmentOverrideRuleRepositoryTests
{
    [Fact]
    public void GetRuleOverridesByLibraryId_LibraryExistsWithOverride_GetsOverrides()
    {
        var libraryId = Guid.NewGuid();
        var treatmentLibrary = TreatmentLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, libraryId);
        var repo = TestHelper.UnitOfWork.TreatmentRuleOverrideRepo;
        var overrideId = Guid.NewGuid();
        var overrideRule = TreatmentOverrideRuleTestSetup.ModelForLibraryEntityInDb(TestHelper.UnitOfWork, libraryId, overrideId);

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
        var overrideRule = TreatmentOverrideRuleTestSetup.ModelForLibraryEntityInDb(TestHelper.UnitOfWork, libraryId, overrideId);
        var rulesBefore = repo.GetRuleOverridesByLibraryId(libraryId);
        Assert.NotEmpty(rulesBefore);

        repo.DeleteRuleOverride(overrideId);

        var rulesAfter = repo.GetRuleOverridesByLibraryId(libraryId);
        Assert.Empty(rulesAfter);
    }
}
