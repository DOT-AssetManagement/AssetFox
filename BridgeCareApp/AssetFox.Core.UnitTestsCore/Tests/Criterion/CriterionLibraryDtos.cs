using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class CriterionLibraryDtos
    {
        public static CriterionLibraryDTO Dto(Guid? id = null, string mergedCriteriaExpression = "mergedCriteriaExpression", string name = null)
        {
            name ??= RandomStrings.WithPrefix("criterionlibrary");
            var resolveId = id ?? Guid.NewGuid();
            var dto = new CriterionLibraryDTO
            {
                Id = resolveId,
                Name = name,
                MergedCriteriaExpression = mergedCriteriaExpression,
                IsSingleUse = true,
            };
            return dto;
        }
    }
}
