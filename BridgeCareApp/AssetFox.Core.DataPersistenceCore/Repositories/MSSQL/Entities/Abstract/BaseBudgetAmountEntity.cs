using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract
{
    public class BaseBudgetAmountEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public int Year { get; set; }

        public decimal Value { get; set; }
    }
}
