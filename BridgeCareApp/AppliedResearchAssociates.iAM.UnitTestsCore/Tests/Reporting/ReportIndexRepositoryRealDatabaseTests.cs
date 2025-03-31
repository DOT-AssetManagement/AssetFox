using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Repositories;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using Xunit;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Reporting
{
    public class ReportIndexRepositoryRealDatabaseTests
    {
        [Fact]
        public void ReportInSystem_GetAllInSystem_Gets()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var simulationId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
            var reportId = Guid.NewGuid();
            var reportIndexDto = new ReportIndexDTO
            {
                ExpirationDate = new DateTime(3025, 3, 21),
                Id = reportId,
                NetworkId = NetworkTestSetup.NetworkId,
                SimulationId = simulationId,
                Result = "Report",
                Type = "Report type",
            };
            TestHelper.UnitOfWork.ReportIndexRepository.Add(reportIndexDto);

            var reportInDatabase = TestHelper.UnitOfWork.ReportIndexRepository.Get(reportId);
            ObjectAssertions.EquivalentExcluding(reportIndexDto, reportInDatabase, r => r.CreationDate);
        }

        [Fact]
        public void ExpiredReportInDb_DeleteExpiredReports_Deletes()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var simulationId = Guid.NewGuid();
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
            var reportId = Guid.NewGuid();
            var reportIndexDto = new ReportIndexDTO
            {
                ExpirationDate = new DateTime(2015, 3, 21),
                Id = reportId,
                NetworkId = NetworkTestSetup.NetworkId,
                SimulationId = simulationId,
                Result = "Report",
                Type = "Report type",
            };
            TestHelper.UnitOfWork.ReportIndexRepository.Add(reportIndexDto);

            TestHelper.UnitOfWork.ReportIndexRepository.DeleteExpiredReports();

            var reportAfter = TestHelper.UnitOfWork.ReportIndexRepository.Get(reportId);
            Assert.Null(reportAfter);
        }
    }
}
