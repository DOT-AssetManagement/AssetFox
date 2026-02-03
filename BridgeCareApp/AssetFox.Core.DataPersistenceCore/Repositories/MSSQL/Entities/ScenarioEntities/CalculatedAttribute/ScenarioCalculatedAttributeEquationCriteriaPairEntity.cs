using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.CalculatedAttribute
{
    public class ScenarioCalculatedAttributeEquationCriteriaPairEntity : BaseCalculatedAttributeEquationPairEntity
    {
        public Guid ScenarioCalculatedAttributeId { get; set; }

        public virtual ScenarioCalculatedAttributeEntity ScenarioCalculatedAttribute { get; set; }

        public virtual ScenarioCriterionLibraryCalculatedAttributePairEntity CriterionLibraryCalculatedAttributeJoin { get; set; }

        public virtual ScenarioEquationCalculatedAttributePairEntity EquationCalculatedAttributeJoin { get; set; }
    }
}
