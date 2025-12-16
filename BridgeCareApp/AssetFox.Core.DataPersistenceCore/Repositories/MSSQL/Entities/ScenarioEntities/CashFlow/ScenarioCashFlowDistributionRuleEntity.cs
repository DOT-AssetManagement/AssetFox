using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.CashFlow
{
    public class ScenarioCashFlowDistributionRuleEntity : BaseCashFlowDistributionRuleEntity
    {
        public Guid ScenarioCashFlowRuleId { get; set; }

        public virtual ScenarioCashFlowRuleEntity ScenarioCashFlowRule { get; set; }
    }
}
