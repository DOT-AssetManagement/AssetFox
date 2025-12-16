using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests.RemainingLifeLimit
{
    public static class RemainingLifeLimitDtos
    {
        public static RemainingLifeLimitDTO Dto(string attribute, Guid? id = null, double value = 0)
        {
            var resolveId = id ?? Guid.NewGuid();
            var dto = new RemainingLifeLimitDTO
            {
                Attribute = attribute,
                Id = resolveId,
                Value = value,
            };
            return dto;
        }

        public static RemainingLifeLimitDTO DtoWithCriterionLibrary(string attribute, Guid? id = null, double value = 0)
        {
            var criterionLibrary = CriterionLibraryDtos.Dto();
            var resolveId = id ?? Guid.NewGuid();
            var dto = new RemainingLifeLimitDTO
            {
                Attribute = attribute,
                Id = resolveId,
                Value = value,
                CriterionLibrary = criterionLibrary
            };
            return dto;
        }
    }
}
