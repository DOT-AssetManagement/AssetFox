using System;
using System.Collections.Generic;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface ICashFlowDistributionRuleRepository
    {
        void UpsertOrDeleteCashFlowDistributionRules(
            Dictionary<Guid, List<CashFlowDistributionRuleDTO>> distributionRulesPerCashFlowRuleId, Guid libraryId);

        void UpsertOrDeleteScenarioCashFlowDistributionRules(
            Dictionary<Guid, List<CashFlowDistributionRuleDTO>> distributionRulesPerCashFlowRuleId, Guid simulationId);
    }
}
