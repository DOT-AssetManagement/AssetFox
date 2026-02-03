using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;
using AssetFoxCore.Services;
using Xunit;

namespace AssetFoxCoreTests
{
    public class PerformanceCurveDtoListHelperTests
    {
        [Fact]
        public void AddModifiedToPerformanceCurve_Does()
        {
            var curve = new PerformanceCurveDTO();
            var curves = new List<PerformanceCurveDTO> { curve };
            Assert.False(curve.IsModified);
            PerformanceCurveDtoListService.AddModifiedToScenarioPerformanceCurve(curves, true);
            Assert.True(curve.IsModified);
        }


        [Fact]
        public void AddLibraryIdToPerformanceCurve_Does()
        {
            var curve = new PerformanceCurveDTO();
            var curves = new List<PerformanceCurveDTO> { curve };
            var libraryId = Guid.NewGuid();

            PerformanceCurveDtoListService.AddLibraryIdToScenarioPerformanceCurves(curves, libraryId);

            Assert.Equal(libraryId, curve.LibraryId);
        }

        [Fact]
        public void AddLibraryIdToPerformanceCurve_LibraryIdIsNull_NoChange()
        {
            var curve = new PerformanceCurveDTO();
            var curves = new List<PerformanceCurveDTO> { curve };
            var libraryId = Guid.NewGuid();
            curve.LibraryId = libraryId;

            PerformanceCurveDtoListService.AddLibraryIdToScenarioPerformanceCurves(curves, null);

            Assert.Equal(libraryId, curve.LibraryId);
        }
    }
}
