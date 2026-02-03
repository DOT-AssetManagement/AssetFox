using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;
using Xunit;

namespace AssetFox.Core.UnitTestsCore.Assertions
{
    public static class CriterionLibraryDtoAssertions
    {
        public static void AssertValidUpsertResult(CriterionLibraryDTO upsertedLibrary, CriterionLibraryDTO loadedCriterionLibrary)
        {
            if (upsertedLibrary == null)
            {
                var emptyLibrary = new CriterionLibraryDTO();
                ObjectAssertions.Equivalent(emptyLibrary, loadedCriterionLibrary);
            }
            else
            {
                ObjectAssertions.EquivalentExcluding(upsertedLibrary, loadedCriterionLibrary,
                    cl => cl.Id,
                    cl => cl.Name,
                    cl => cl.IsSingleUse,
                    cl => cl.Owner);
                Assert.True(loadedCriterionLibrary.IsSingleUse);
            }
        }
    }
}
