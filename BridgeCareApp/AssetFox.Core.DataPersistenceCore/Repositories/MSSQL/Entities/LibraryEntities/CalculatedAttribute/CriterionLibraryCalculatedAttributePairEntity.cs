using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.CalculatedAttribute
{
    public class CriterionLibraryCalculatedAttributePairEntity : BaseCriterionLibraryJoinEntity
    {
        public Guid CalculatedAttributePairId { get; set; }

        public virtual CalculatedAttributeEquationCriteriaPairEntity CalculatedAttributePair { get; set; }
    }
}
