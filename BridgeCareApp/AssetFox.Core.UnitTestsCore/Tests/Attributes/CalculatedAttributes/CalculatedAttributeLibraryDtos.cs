using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class CalculatedAttributeLibraryDtos
    {
        public static CalculatedAttributeLibraryDTO Dto(Guid? libraryId = null, string name = null)
        {
            var resolveLibraryId = libraryId ?? Guid.NewGuid();
            var randomName = name ?? RandomStrings.WithPrefix("CalcAttr");
            var calcAttr = new CalculatedAttributeLibraryDTO
            {
                Id = resolveLibraryId,
                Name = randomName,
                Description = "test library"
            };
            return calcAttr;
        }
    }
}
