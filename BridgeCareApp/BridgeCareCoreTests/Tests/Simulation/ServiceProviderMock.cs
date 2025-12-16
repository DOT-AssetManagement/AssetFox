using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFoxCore.Interfaces;
using Moq;

namespace AssetFoxCoreTests.Tests
{
    public static class ServiceProviderMock
    {
        public static Mock<IServiceProvider> New()
        {
            var mock = new Mock<IServiceProvider>();
            return mock;
        }
    }
}
