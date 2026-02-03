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
    public class CashFlowRuleDtoListHelperTests
    {
        [Fact]
        public void AddModifiedToCashFlowRule_Does()
        {
            var curve = new CashFlowRuleDTO();
            var curves = new List<CashFlowRuleDTO> { curve };
            Assert.False(curve.IsModified);
            CashFlowRuleDtoListService.AddModifiedToScenarioCashFlowRule(curves, true);
            Assert.True(curve.IsModified);
        }


        [Fact]
        public void AddLibraryIdToCashFlowRule_Does()
        {
            var curve = new CashFlowRuleDTO();
            var curves = new List<CashFlowRuleDTO> { curve };
            var libraryId = Guid.NewGuid();

            CashFlowRuleDtoListService.AddLibraryIdToScenarioCashFlowRule(curves, libraryId);

            Assert.Equal(libraryId, curve.LibraryId);
        }

        [Fact]
        public void AddLibraryIdToCashFlowRule_LibraryIdIsNull_NoChange()
        {
            var curve = new CashFlowRuleDTO();
            var curves = new List<CashFlowRuleDTO> { curve };
            var libraryId = Guid.NewGuid();
            curve.LibraryId = libraryId;

            CashFlowRuleDtoListService.AddLibraryIdToScenarioCashFlowRule(curves, null);

            Assert.Equal(libraryId, curve.LibraryId);
        }
    }
}
