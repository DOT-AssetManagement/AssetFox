using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests.TreatmentOverrideRuleRepository;

public static class TreatmentOverrideRuleTestSetup
{
    public static TreatmentRuleOverrideDTO ModelForLibraryEntityInDb(IUnitOfWork unitOfWork, Guid libraryId, Guid overrideId)
    {
        var ruleOverride = CreateOverrideRule(overrideId);
        var ruleOverrides = new List<TreatmentRuleOverrideDTO> { ruleOverride };
        unitOfWork.TreatmentRuleOverrideRepo.UpsertRuleOverrides(libraryId, ruleOverrides);
        return ruleOverride;
    }

    private static TreatmentRuleOverrideDTO CreateOverrideRule(Guid overrideId)
    {
        var ruleOverride = new TreatmentRuleOverrideDTO
        {
            Id = overrideId,
            Name = "Test Override",
            Description = "Test Override Description",
            SourceTreatmentRuleOverrideId = null,
            IsEnabledByDefault = true,
            IsEnabled = true,
        };
        return ruleOverride;
    }
}
