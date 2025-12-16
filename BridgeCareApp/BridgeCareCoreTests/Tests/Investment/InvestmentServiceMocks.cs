using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFoxCore.Interfaces;
using Moq;

namespace AssetFoxCoreTests.Tests
{
    public static class InvestmentBudgetServiceMocks { 
  
        public static Mock<IInvestmentBudgetsService> New()
        {
            var mock = new Mock<IInvestmentBudgetsService>();
            return mock;
        }
    }
}
