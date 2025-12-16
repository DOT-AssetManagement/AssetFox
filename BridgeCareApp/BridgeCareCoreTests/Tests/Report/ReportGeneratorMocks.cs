using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Reporting;
using Moq;

namespace AssetFoxCoreTests.Tests.Report
{
    public static class ReportGeneratorMocks
    {
        public static Mock<IReportGenerator> New()
        {
            var mock = new Mock<IReportGenerator>();
            return mock;
        }
    }
}
