using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Analysis.Engine;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class BudgetDetails
    {
        public static BudgetDetail Detail()
        {
            var amount = (decimal)123;
            var detail = new BudgetDetail(amount, "Budget");
            return detail;
        }
    }
}
