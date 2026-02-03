using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFoxCore.Interfaces;
using Moq;

namespace AssetFoxCoreTests.Tests.PerformanceCurve
{
    public class PerformanceCurvesPagingServiceMocks
    {
        public static Mock<IPerformanceCurvesPagingService> New()
        {
            var mock = new Mock<IPerformanceCurvesPagingService>();
            return mock;
        }
    }
}
