using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.CalculatedAttribute
{
    public class EquationCalculatedAttributePairEntity : BaseEquationJoinEntity
    {
        public Guid CalculatedAttributePairId { get; set; }

        public virtual CalculatedAttributeEquationCriteriaPairEntity CalculatedAttributePair { get; set; }
    }
}
