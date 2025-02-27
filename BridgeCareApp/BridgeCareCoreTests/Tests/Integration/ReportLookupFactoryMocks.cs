using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.Reporting;
using Moq;

namespace BridgeCareCoreTests.Tests
{
    public static class ReportLookupFactoryMocks
    {
        public static Mock<IReportLookupLibrary> Default()
        {
            var mock = new Mock<IReportLookupLibrary>();
            return mock;
        }
    }
}
