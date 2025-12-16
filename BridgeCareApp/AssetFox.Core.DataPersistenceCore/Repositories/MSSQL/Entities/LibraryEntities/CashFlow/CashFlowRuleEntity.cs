using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.CashFlow
{
    public class CashFlowRuleEntity : BaseCashFlowRuleEntity
    {
        public CashFlowRuleEntity()
        {
            CashFlowDistributionRules = new HashSet<CashFlowDistributionRuleEntity>();
        }

        public Guid CashFlowRuleLibraryId { get; set; }

        public virtual CashFlowRuleLibraryEntity CashFlowRuleLibrary { get; set; }

        public virtual CriterionLibraryCashFlowRuleEntity CriterionLibraryCashFlowRuleJoin { get; set; }

        public virtual ICollection<CashFlowDistributionRuleEntity> CashFlowDistributionRules { get; set; }
    }
}
