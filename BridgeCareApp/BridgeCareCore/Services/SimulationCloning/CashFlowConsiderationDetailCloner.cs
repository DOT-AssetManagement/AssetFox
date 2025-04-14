using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs;

namespace BridgeCareCore.Services.SimulationCloning
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
