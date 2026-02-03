using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.TestHelpers.Assertions;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.TestUtils;
using Xunit;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public class SimulationLogRepositoryTests
    {
        [Fact]
        public async Task CreateLog_ThenGet_Expected()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var simulationId = Guid.NewGuid();
            SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
            var logs = new List<SimulationLogDTO> { SimulationLogDtos.Dto(simulationId) };
            var before = DateTime.Now;

            TestHelper.UnitOfWork.SimulationLogRepo.CreateLog(logs);

            var after = DateTime.Now;
            var logsAfter = await TestHelper.UnitOfWork.SimulationLogRepo.GetLog(simulationId);
            for (int index = 0;index<logs.Count;index++)
            {
                ObjectAssertions.EquivalentExcluding(logs[index], logsAfter[index], l => l.TimeStamp, l => l.Id);
                Assert.NotEqual(logs[index].Id, logsAfter[index].Id);
                DateTimeAssertions.Between(before, after, logsAfter[index].TimeStamp, TimeSpan.FromSeconds(1));
            }            
        }

        [Fact]
        public async Task ClearLog_LogInDb_Deletes()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var simulationId = Guid.NewGuid();
            SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId);
            var logs = new List<SimulationLogDTO> { SimulationLogDtos.Dto(simulationId) };
            TestHelper.UnitOfWork.SimulationLogRepo.CreateLog(logs);
            var logsBefore = await TestHelper.UnitOfWork.SimulationLogRepo.GetLog(simulationId);
            Assert.Single(logsBefore);

            TestHelper.UnitOfWork.SimulationLogRepo.ClearLog(simulationId);

            var logsAfter = await TestHelper.UnitOfWork.SimulationLogRepo.GetLog(simulationId);
            Assert.Empty(logsAfter);
        }
    }
}
