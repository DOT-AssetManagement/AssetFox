using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract
{
    public class BaseEquationJoinEntity : BaseEntity
    {
        public Guid EquationId { get; set; }

        public virtual EquationEntity Equation { get; set; }
    }
}
