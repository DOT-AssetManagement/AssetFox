using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.TestUtils;
using Xunit;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public class SimulationReportDetailRepositoryTests
    {
        [Fact]
        public void UpsertSimulationReportDetail_Does()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var simulationId = Guid.NewGuid();
            SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
            var dto = SimulationReportDetailDtos.Dto(simulationId);

            TestHelper.UnitOfWork.SimulationReportDetailRepo.UpsertSimulationReportDetail(dto);

            var entityAfter = TestHelper.UnitOfWork.Context.SimulationReportDetail.Single(srd => srd.SimulationId == simulationId);
            Assert.Equal(dto.Status, entityAfter.Status);
        }
    }
}
