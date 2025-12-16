using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Reporting;
using Moq;

namespace AssetFoxCoreTests.Tests
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
