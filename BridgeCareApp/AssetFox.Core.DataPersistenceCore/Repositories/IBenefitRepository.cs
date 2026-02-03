using System;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IBenefitRepository
    {
        void UpsertBenefit(BenefitDTO dto, Guid analysisMethodId);
    }
}
