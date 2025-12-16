using System.Collections.Generic;
using AssetFox.Core.Analysis;
using AssetFox.Core.DataPersistenceCore.Migrations;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Budget
{
    public class BudgetLibraryEntity : LibraryEntity
    {
        public BudgetLibraryEntity()
        {
            Budgets = new HashSet<BudgetEntity>();
            Users = new HashSet<BudgetLibraryUserEntity>();
        }

        public virtual ICollection<BudgetEntity> Budgets { get; set; }

        public virtual ICollection<BudgetLibraryUserEntity> Users { get; set; }
    }
}
