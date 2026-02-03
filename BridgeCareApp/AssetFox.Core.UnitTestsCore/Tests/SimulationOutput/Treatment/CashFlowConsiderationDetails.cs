using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Analysis.Engine;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class CashFlowConsiderationDetails
    {
        public static CashFlowConsiderationDetail Detail()
        {
            var detail = new CashFlowConsiderationDetail("CashFlowRuleName")
            {
                ReasonAgainstCashFlow = ReasonAgainstCashFlow.Undefined,
            };
            return detail;
        }
    }
}
