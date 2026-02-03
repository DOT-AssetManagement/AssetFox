using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests.Reporting
{
    public class ReportIndexRepositoryTests
    {
        private UnitOfDataPersistenceWork _testRepo;
        private IQueryable<ReportIndexEntity> _testReportIndexList;
        private Mock<IAMContext> _mockedContext;
        private Mock<DbSet<ReportIndexEntity>> _mockedReportIndexSet;

        public ReportIndexRepositoryTests()
        {
            _mockedContext = new Mock<IAMContext>();
            _testReportIndexList = TestDataForReportIndex.SimpleRepo().AsQueryable();

            // From https://docs.microsoft.com/en-us/ef/ef6/fundamentals/testing/mocking?redirectedfrom=MSDN
            _mockedReportIndexSet = new Mock<DbSet<ReportIndexEntity>>();
            _mockedReportIndexSet.As<IQueryable<ReportIndexEntity>>().Setup(_ => _.Provider).Returns(_testReportIndexList.Provider);
            _mockedReportIndexSet.As<IQueryable<ReportIndexEntity>>().Setup(_ => _.Expression).Returns(_testReportIndexList.Expression);
            _mockedReportIndexSet.As<IQueryable<ReportIndexEntity>>().Setup(_ => _.ElementType).Returns(_testReportIndexList.ElementType);
            _mockedReportIndexSet.As<IQueryable<ReportIndexEntity>>().Setup(_ => _.GetEnumerator()).Returns(_testReportIndexList.GetEnumerator());

            _mockedContext.Setup(_ => _.ReportIndex).Returns(_mockedReportIndexSet.Object);
            _mockedContext.Setup(_ => _.Set<ReportIndexEntity>()).Returns(_mockedReportIndexSet.Object);
            var mockedRepo = new UnitOfDataPersistenceWork((new Mock<IConfiguration>()).Object, _mockedContext.Object);
            _testRepo = mockedRepo;
        }

        [Fact]
        public void AddSuccessfullyInsertsNewReport()
        {
            // Arrange
            var newReport = new ReportIndexDTO
            {
                Id = new Guid("5ef4090a-77d6-4ed9-9fe1-6a938e043137"),
                SimulationId = TestDataForReportIndex.SimulationIdB,
                Type = "Test Report File",
                Result = "C:\\fakepath\\report.xlsx",
                ExpirationDate = DateTime.Now.AddDays(2)
            };
            var repo = new ReportIndexRepository(_testRepo);

            // Act
            repo.Add(newReport);

            // Assert
            _mockedReportIndexSet.Verify(_ => _.Add(It.IsAny<ReportIndexEntity>()), Times.Once());
            _mockedContext.Verify(_ => _.SaveChanges(), Times.Once());
        }

        [Fact]
        public void AddSuccessfullyReplacesExistingReport()
        {
            // Arrange
            var newReport = new ReportIndexDTO()
            {
                Id = TestDataForReportIndex.ReportId2,
                SimulationId = TestDataForReportIndex.SimulationIdA,
                Type = "Test Report File",
                Result = "C:\\fakepath\\report.xlsx",
                ExpirationDate = DateTime.Now.AddDays(2)
            };
            var repo = new ReportIndexRepository(_testRepo);

            // Act
            repo.Add(newReport);

            // Assert
            _mockedReportIndexSet.Verify(_ => _.Remove(It.IsAny<ReportIndexEntity>()), Times.Once());
            _mockedReportIndexSet.Verify(_ => _.Add(It.IsAny<ReportIndexEntity>()), Times.Once());
            _mockedContext.Verify(_ => _.SaveChanges(), Times.Exactly(2));
        }

        [Fact]
        public void Get_ReportExists_Gets()
        {
            var repo = new ReportIndexRepository(_testRepo);
            var reportId = TestDataForReportIndex.ReportId1;

            var result = repo.Get(reportId);

            Assert.Equal("Test HTML File", result.Type);
        }

        [Fact]
        public void GetAllForScenario_ReportsExist_Gets()
        {
            var repo = new ReportIndexRepository(_testRepo);

            var reports = repo.GetAllForScenario(TestDataForReportIndex.SimulationIdA);

            Assert.Equal(2, reports.Count);
        }

        [Fact]
        public void AddHandlesMissingId()
        {
            // Arrange
            var newReport = new ReportIndexDTO()
            {
                Id = Guid.Empty,
                SimulationId = null,
                Type = "Test Report File",
                Result = "C:\\fakepath\\report.xlsx",
                ExpirationDate = DateTime.Now.AddDays(2)
            };
            var repo = new ReportIndexRepository(_testRepo);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => repo.Add(newReport));
        }

        [Fact]
        public void AddHandlesMissingreportType()
        {
            // Arrange
            var newReport = new ReportIndexDTO()
            {
                Id = new Guid("5ef4090a-77d6-4ed9-9fe1-6a938e043137"),
                SimulationId = TestDataForReportIndex.SimulationIdB,
                Type = "",
                Result = "C:\\fakepath\\report.xlsx",
                ExpirationDate = DateTime.Now.AddDays(2)
            };
            var repo = new ReportIndexRepository(_testRepo);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => repo.Add(newReport));
        }

        // This cannot be unit tested.  The DeleteAll extension on the context fails
        //[Fact]
        //public void DASRDeletesAllScenarioReports()
        //{
        //    // Arrange
        //    var repo = new ReportIndexRepository(_testRepo);
        //    var testScenario = new Guid("be82f095-c108-4ab7-af7e-cb7ecd18ede2");

        //    // Act
        //    var returnVal = repo.DeleteAllSimulationReports(testScenario);

        //    // Assert
        //    _mockedReportIndexSet.Verify(_ => _.RemoveRange(It.IsAny<IList<ReportIndexEntity>>()), Times.Once());
        //    _mockedContext.Verify(_ => _.SaveChanges(), Times.Once());
        //    Assert.True(returnVal);
        //}

        [Fact]
        public void DASRHandlesScenaroIdDoesNotExist()
        {
            // Arrange
            var repo = new ReportIndexRepository(_testRepo);
            var testScenario = new Guid("be82f095-aaaa-aaaa-aaaa-cb7ecd18ede2");

            // Act
            var returnVal = repo.DeleteAllSimulationReports(testScenario);

            // Assert
            _mockedReportIndexSet.Verify(_ => _.RemoveRange(It.IsAny<IList<ReportIndexEntity>>()), Times.Never());
            _mockedContext.Verify(_ => _.SaveChanges(), Times.Never());
            Assert.False(returnVal);
        }

        // This cannot be unit tested.  The DeleteAll extension on the context fails
        //[Fact]
        //public void ExpiredReportsDeleted()
        //{
        //    // Arrange
        //    var repo = new ReportIndexRepository(_testRepo);

        //    // Act
        //    var returnVal = repo.DeleteExpiredReports();

        //    // Assert
        //    _mockedReportIndexSet.Verify(_ => _.RemoveRange(It.IsAny<IList<ReportIndexEntity>>()), Times.Once());
        //    _mockedContext.Verify(_ => _.SaveChanges(), Times.Once());
        //    Assert.True(returnVal);
        //}

        [Fact]
        public void SpecificReportDeleted()
        {
            // Arrange
            var repo = new ReportIndexRepository(_testRepo);
            var testReport = TestDataForReportIndex.ReportId2;

            // Act
            var returnVal = repo.DeleteReport(testReport);

            // Assert
            _mockedReportIndexSet.Verify(_ => _.Remove(It.IsAny<ReportIndexEntity>()), Times.Once());
        }

        [Fact]
        public void DeleteReportHandlesScenarioIdNotExist()
        {
            // Arrange
            var repo = new ReportIndexRepository(_testRepo);
            var testReport = new Guid("be82f095-aaaa-aaaa-aaaa-cb7ecd18ede2");

            // Act
            var returnVal = repo.DeleteReport(testReport);

            // Assert
            _mockedReportIndexSet.Verify(_ => _.Remove(It.IsAny<ReportIndexEntity>()), Times.Never());
        }
    }
}
