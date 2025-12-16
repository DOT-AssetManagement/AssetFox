using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Budget
{
    public class BudgetAmountEntity : BaseBudgetAmountEntity
    {
        public Guid BudgetId { get; set; }

        public virtual BudgetEntity Budget { get; set; }
    }
}
