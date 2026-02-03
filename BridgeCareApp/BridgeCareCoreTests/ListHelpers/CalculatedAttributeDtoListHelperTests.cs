using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Services;
using Xunit;

namespace AssetFoxCoreTests.ListHelpers
{
    public class CalculatedAttributeDtoListHelperTests
    {
        [Fact]
        public void AddModifiedToCalculatedAttribute_Does()
        {
            var curve = new CalculatedAttributeDTO();
            var curves = new List<CalculatedAttributeDTO> { curve };
            Assert.False(curve.IsModified);
            CalculatedAttributeDtoListService.AddModifiedToScenarioCalculatedAttributes(curves, true);
            Assert.True(curve.IsModified);
        }


        [Fact]
        public void AddLibraryIdToCalculatedAttribute_Does()
        {
            var curve = new CalculatedAttributeDTO();
            var curves = new List<CalculatedAttributeDTO> { curve };
            var libraryId = Guid.NewGuid();

            CalculatedAttributeDtoListService.AddLibraryIdToScenarioCalculatedAttributes(curves, libraryId);

            Assert.Equal(libraryId, curve.LibraryId);
        }

        [Fact]
        public void AddLibraryIdToCalculatedAttribute_LibraryIdIsNull_NoChange()
        {
            var curve = new CalculatedAttributeDTO();
            var curves = new List<CalculatedAttributeDTO> { curve };
            var libraryId = Guid.NewGuid();
            curve.LibraryId = libraryId;

            CalculatedAttributeDtoListService.AddLibraryIdToScenarioCalculatedAttributes(curves, null);

            Assert.Equal(libraryId, curve.LibraryId);
        }
    }
}
