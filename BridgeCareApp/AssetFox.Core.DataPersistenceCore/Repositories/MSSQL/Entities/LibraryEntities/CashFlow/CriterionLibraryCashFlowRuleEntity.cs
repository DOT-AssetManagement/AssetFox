using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.CashFlow
{
    public class CriterionLibraryCashFlowRuleEntity : BaseCriterionLibraryJoinEntity
    {
        public Guid CashFlowRuleId { get; set; }

        public virtual CashFlowRuleEntity CashFlowRule { get; set; }
    }
}
