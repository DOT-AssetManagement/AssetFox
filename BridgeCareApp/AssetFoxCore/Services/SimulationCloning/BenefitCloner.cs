using System;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services
{

    internal class BenefitCloner
    {
        internal static BenefitDTO Clone(BenefitDTO benefit)
        {
            var newBenefitId = benefit.Id == Guid.Empty ? Guid.Empty : Guid.NewGuid();
            var clone = new BenefitDTO
            {
                Id = newBenefitId,
                Limit = benefit.Limit,
                Attribute = benefit.Attribute,
            };
            return clone;
        }

    }
}

