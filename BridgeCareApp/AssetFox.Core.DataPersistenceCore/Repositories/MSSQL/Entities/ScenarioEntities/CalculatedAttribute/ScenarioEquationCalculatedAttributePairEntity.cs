using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.CalculatedAttribute
{
    public class ScenarioEquationCalculatedAttributePairEntity : BaseEquationJoinEntity
    {
        public Guid ScenarioCalculatedAttributePairId { get; set; }

        public virtual ScenarioCalculatedAttributeEquationCriteriaPairEntity ScenarioCalculatedAttributePair { get; set; }
    }
}
