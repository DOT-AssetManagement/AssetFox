using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFoxCore.Interfaces;
using Moq;

namespace AssetFox.Core.UnitTestsCore.TestUtils
{
    public static class TreatmentServiceMocks
    {
        public static Mock<ITreatmentService> EmptyMock => new Mock<ITreatmentService>();
    }
}
