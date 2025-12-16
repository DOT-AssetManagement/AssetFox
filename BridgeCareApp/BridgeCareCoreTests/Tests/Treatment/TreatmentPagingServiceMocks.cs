using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFoxCore.Interfaces;
using Moq;

namespace AssetFoxCoreTests.Tests.Treatment
{
    public static class TreatmentPagingServiceMocks
    {
        public static Mock<ITreatmentPagingService> EmptyMock => new Mock<ITreatmentPagingService>();

    }
}
