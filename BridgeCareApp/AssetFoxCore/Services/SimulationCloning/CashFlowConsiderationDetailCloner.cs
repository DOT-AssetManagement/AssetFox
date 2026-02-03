using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.SimulationCloning
{
    public class CashFlowConsiderationDetailCloner
    {
        internal static IList<CashFlowConsiderationDetailDTO> CloneList(IList<CashFlowConsiderationDetailDTO> cashFlowConsiderations)
        {
            var cloneList = new List<CashFlowConsiderationDetailDTO>();

            foreach (var cashFlowConsideration in cashFlowConsiderations)
            {
                cloneList.Add(Clone(cashFlowConsideration));
            }

            return cloneList;
        }

        internal static CashFlowConsiderationDetailDTO Clone(CashFlowConsiderationDetailDTO cashFlowConsideration)
        {
            return new CashFlowConsiderationDetailDTO
            {
                Id = Guid.NewGuid(),
                CashFlowRuleName = cashFlowConsideration.CashFlowRuleName,
                ReasonAgainstCashFlow = cashFlowConsideration.ReasonAgainstCashFlow
            };
        }
    }
}
