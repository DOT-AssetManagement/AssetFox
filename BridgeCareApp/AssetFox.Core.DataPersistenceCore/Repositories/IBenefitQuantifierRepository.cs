using System;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IBenefitQuantifierRepository
    {
        BenefitQuantifierDTO GetBenefitQuantifier(Guid networkId);

        void UpsertBenefitQuantifierNonAtomic(BenefitQuantifierDTO dto);

        void DeleteBenefitQuantifier(Guid networkId);
        void UpsertBenefitQuantifier(BenefitQuantifierDTO dto);
    }
}
