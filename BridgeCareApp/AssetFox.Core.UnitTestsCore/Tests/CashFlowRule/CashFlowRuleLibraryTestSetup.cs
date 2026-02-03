using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;

namespace AssetFox.Core.UnitTestsCore.Tests.CashFlowRule
{
    public static class CashFlowRuleLibraryTestSetup
    {
        private static CashFlowRuleDTO CreateCashFlowRuleDto(string cashFlowRuleName)
        {
            //create budget amounts
            //var budgetAmountList = new List<BudgetAmountDTO>();
            //budgetAmountList.Add(CreateBudgetAmountObject(budgetName, 2010, 2010000));

            //create criterion library
            //var criterionLibraryObject = CreateCriterionLibraryObject("0=0", true);

            return new CashFlowRuleDTO()
            {
                Id = Guid.NewGuid(),
                Name = cashFlowRuleName,
                //BudgetAmounts = budgetAmountList,
                //CriterionLibrary = criterionLibraryObject
            };
        }
        public static CashFlowRuleLibraryDTO CreateCashFlowRuleLibraryDto(string name)
        {
            //setup
            var cashFlowRuleList = new List<CashFlowRuleDTO>();
            cashFlowRuleList.Add(CreateCashFlowRuleDto("Cash Flow Rule 1"));

            //create cash flow rule library
            return new CashFlowRuleLibraryDTO()
            {
                Id = Guid.NewGuid(),
                Name = name,
                CashFlowRules = cashFlowRuleList?.ToList()
            };
        }
        public static CashFlowRuleLibraryDTO ModelForEntityInDb(IUnitOfWork unitOfWork, string cashFlowRuleLibraryName = null)
        {
            var resolveCashFlowRuleLibraryName = cashFlowRuleLibraryName ?? RandomStrings.WithPrefix("CashFlowRuleLibrary");
            var dto = CreateCashFlowRuleLibraryDto(resolveCashFlowRuleLibraryName);
            unitOfWork.CashFlowRuleRepo.UpsertCashFlowRuleLibrary(dto);
            var dtoAfter = unitOfWork.CashFlowRuleRepo.GetCashFlowRuleLibraries().FirstOrDefault(x => x.Id == dto.Id);
            return dtoAfter;
        }
    }
}
