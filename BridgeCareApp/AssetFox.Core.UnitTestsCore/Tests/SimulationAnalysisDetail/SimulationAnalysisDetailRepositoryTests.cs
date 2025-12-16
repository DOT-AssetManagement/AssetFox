using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.TestUtils;
using Xunit;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public class SimulationAnalysisDetailRepositoryTests
    {
        [Fact]
        public void UpsertSimulationAnalysisDetail_ThenGet_Same()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var simulationId = Guid.NewGuid();
            SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
            var dto = SimulationAnalysisDetailDtos.ForSimulation(simulationId);

            TestHelper.UnitOfWork.SimulationAnalysisDetailRepo.UpsertSimulationAnalysisDetail(dto);

            var dtoAfter = TestHelper.UnitOfWork.SimulationAnalysisDetailRepo.GetSimulationAnalysisDetail(simulationId);
            ObjectAssertions.Equivalent(dtoAfter, dto);
        }
    }
}
