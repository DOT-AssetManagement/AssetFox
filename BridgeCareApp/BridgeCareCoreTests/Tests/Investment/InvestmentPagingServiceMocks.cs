using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFoxCore.Interfaces;
using Moq;

namespace AssetFoxCoreTests.Tests.Investment
{
    internal static class InvestmentPagingServiceMocks
    {
        public static Mock<IInvestmentPagingService> DefaultMock()
        {
            return new Mock<IInvestmentPagingService>();
        }
    }
}
