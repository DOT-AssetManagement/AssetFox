using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.Reporting;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.UnitTestsCore.TestUtils;

namespace AssetFox.Core.UnitTestsCore.Tests.Reporting
{
    public class ReportGeneratorTests
    {
        private ReportLookupLibrary _testReportLibrary;
        private UnitOfDataPersistenceWork _testRepo;
        private DictionaryBasedReportGenerator _generator;

        public ReportGeneratorTests()
        {
            var mockedContext = new Mock<IAMContext>();
            var testReportIndexList = TestDataForReportIndex.SimpleRepo().AsQueryable();
            var mockedReportIndexSet = MockedContextBuilder.AddDataSet(mockedContext, _ => _.ReportIndex, testReportIndexList);
            var mockedRepo = new UnitOfDataPersistenceWork((new Mock<IConfiguration>()).Object, mockedContext.Object);
            _testRepo = mockedRepo;

            _testReportLibrary = new ReportLookupLibrary(TestDataForReportIndex.SimpleReportLibrary());
            var hubService = HubServiceMocks.Default();

            _generator = new DictionaryBasedReportGenerator(_testRepo, _testReportLibrary, hubService);
        }

        [Fact]
        public async Task GeneratorCanGenerateReportInLibrary()
        {
            // Arrange
            string goodReport = "Test Report File";

            // Act
            IReport report = await _generator.Generate(goodReport);

            // Assert
            Assert.Equal(ReportType.File, report.Type);
            Assert.Equal(goodReport, report.ReportTypeName);
            Assert.Equal(0, report.Errors.Count());
        }

        [Fact]
        public async Task GeneratorReturnsFailureReportWhenReportNotInLibrary()
        {
            // Arrange
            string badReport = "Some missing report";

            // Act
            IReport report = await _generator.Generate(badReport);

            // Assert
            Assert.Equal(ReportType.HTML, report.Type);
            Assert.Equal("Failure Report", report.ReportTypeName);
            Assert.True(report.Errors.Count() > 0);
        }

        [Fact]
        public void GeneratorReturnsAllScenarioReports()
        {
            // Arrange
            Guid scenarioId = TestDataForReportIndex.SimulationIdA;

            // Act
            var reportList = _generator.GetAllReportsForScenario(scenarioId);

            // Assert
            Assert.Equal(2, reportList.Count());
        }

        [Fact]
        public void GeneratorHandlesAScenarioWithoutReports()
        {
            // Arrange
            Guid scenarioId = new Guid("be82f095-aaaa-aaaa-aaaa-cb7ecd18ede2"); // Should not exist in demo repo

            // Act
            var reportList = _generator.GetAllReportsForScenario(scenarioId);

            // Assert
            Assert.Empty(reportList);
        }

        [Fact]
        public async Task GeneratorSuccessfullyReturnsASpecificReport()
        {
            // Arrange
            Guid reportId = TestDataForReportIndex.ReportId3;

            // Act
            var report = await _generator.GetExisting(reportId);

            // Assert
            Assert.Equal(TestDataForReportIndex.SimulationIdB, report.SimulationID);
        }

        [Fact]
        public async Task GeneratorHandlesNotFindingASpecificReport()
        {
            // Arrange
            Guid reportId = new Guid("be82f095-aaaa-aaaa-aaaa-cb7ecd18ede2"); // Should not exist in demo repo

            // Act
            var report = await _generator.GetExisting(reportId);
        }
    }

    
}
