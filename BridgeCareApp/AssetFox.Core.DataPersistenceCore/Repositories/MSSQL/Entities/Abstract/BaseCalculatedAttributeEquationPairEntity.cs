using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract
{
    public abstract class BaseCalculatedAttributeEquationPairEntity : BaseEntity
    {
        public Guid Id { get; set; }
    }
}
