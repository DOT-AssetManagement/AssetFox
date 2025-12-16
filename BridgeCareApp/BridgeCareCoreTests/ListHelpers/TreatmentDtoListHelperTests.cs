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
    public class TreatmentDtoListHelperTests
    {
        [Fact]
        public void AddModifiedToTreatment_Does()
        {
            var curve = new TreatmentDTO();
            var curves = new List<TreatmentDTO> { curve };
            Assert.False(curve.IsModified);
            TreatmentDtoListService.AddModifiedToScenarioSelectableTreatments(curves, true);
            Assert.True(curve.IsModified);
        }


        [Fact]
        public void AddLibraryIdToTreatment_Does()
        {
            var curve = new TreatmentDTO();
            var curves = new List<TreatmentDTO> { curve };
            var libraryId = Guid.NewGuid();

            TreatmentDtoListService.AddLibraryIdToScenarioSelectableTreatments(curves, libraryId);

            Assert.Equal(libraryId, curve.LibraryId);
        }

        [Fact]
        public void AddLibraryIdToTreatment_LibraryIdIsNull_NoChange()
        {
            var curve = new TreatmentDTO();
            var curves = new List<TreatmentDTO> { curve };
            var libraryId = Guid.NewGuid();
            curve.LibraryId = libraryId;

            TreatmentDtoListService.AddLibraryIdToScenarioSelectableTreatments(curves, null);

            Assert.Equal(libraryId, curve.LibraryId);
        }
    }
}
