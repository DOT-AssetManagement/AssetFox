using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;

namespace AssetFox.Core.UnitTestsCore.Tests.TreatmentOverrideRuleRepository;

public static class TreatmentRuleOverrideTestSetup
{
    public static TreatmentRuleOverrideDTO ModelForLibraryEntityInDb(IUnitOfWork unitOfWork, Guid libraryId, Guid overrideId, string name = null)
    {
        name ??= RandomStrings.WithPrefix("RuleOverride_");
        var ruleOverride = CreateOverrideRule(overrideId, name);
        var ruleOverrides = new List<TreatmentRuleOverrideDTO> { ruleOverride };
        unitOfWork.TreatmentRuleOverrideRepo.UpsertRuleOverrides(libraryId, ruleOverrides);
        return ruleOverride;
    }

    private static TreatmentRuleOverrideDTO CreateOverrideRule(Guid overrideId, string name)
    {
        var ruleOverride = new TreatmentRuleOverrideDTO
        {
            Id = overrideId,
            Name = name,
            Description = "Test Override Description",
            SourceTreatmentRuleOverrideId = null,
            IsEnabledByDefault = true,
            IsEnabled = true,
        };
        return ruleOverride;
    }
}
