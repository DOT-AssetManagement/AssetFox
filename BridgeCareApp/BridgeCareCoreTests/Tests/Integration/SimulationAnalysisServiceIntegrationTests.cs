using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.Tests;
using AssetFox.Core.UnitTestsCore.TestUtils;
using Xunit;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using AssetFoxCore.Services;
using AssetFox.Core.UnitTestsCore.Tests.User;
using AssetFoxCore.Models;
using System.Data;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.WorkQueue;

namespace AssetFoxCoreTests.Tests.Integration
{
    public class SimulationAnalysisServiceIntegrationTests
    {
        private GeneralWorkQueueService CreateService()
        {
            var queue = new SequentialWorkQueue<WorkQueueMetadata>();
            var fastQueue = new FastSequentialworkQueue<WorkQueueMetadata>();
            var hiddenUploadQueue = new HiddenUploadQueue<WorkQueueMetadata>();
            var service = new GeneralWorkQueueService(
                queue, fastQueue, hiddenUploadQueue);
            return service;
        }

        [Fact]
        public async Task CreateAndRunPermitted_SimulationExists_Ok()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var simulationId = Guid.NewGuid();
            var simulationName = RandomStrings.WithPrefix("Simulation");
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            TestHelper.UnitOfWork.SetUser(user.Username);
            var userId = user.Id;
            SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, simulationId, simulationName, userId);
            var service = CreateService();

            var userInfo = new UserInfo
            {
                Name = user.Username,
                HasAdminAccess = true,
                HasSimulationAccess = true,
                Email = "Foo@bar.Com",
            };
            var expectedWorkItem = new AnalysisWorkItem(
                NetworkTestSetup.NetworkId, simulationId, userInfo, simulationName);

            var result = service.CreateAndRun(expectedWorkItem);
            var resultUser = result.UserId;
            Assert.Equal(user.Username, resultUser);
        }        
    }
}
