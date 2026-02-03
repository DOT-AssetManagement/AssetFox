using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.CalculatedAttribute
{
    public class CalculatedAttributeEquationCriteriaPairEntity : BaseCalculatedAttributeEquationPairEntity
    {
        public Guid CalculatedAttributeId { get; set; }

        public virtual CalculatedAttributeEntity CalculatedAttribute { get; set; }

        public virtual CriterionLibraryCalculatedAttributePairEntity CriterionLibraryCalculatedAttributeJoin { get; set; }

        public virtual EquationCalculatedAttributePairEntity EquationCalculatedAttributeJoin { get; set; }
    }
}
