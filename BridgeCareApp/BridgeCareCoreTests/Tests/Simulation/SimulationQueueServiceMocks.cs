using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFoxCore.Interfaces;
using Moq;

namespace AssetFoxCoreTests.Tests
{
    public static class SimulationQueueServiceMocks
    {
        public static Mock<IWorkQueueService> New()
        {
            var mock = new Mock<IWorkQueueService>();
            return mock;
        }
    }
}
