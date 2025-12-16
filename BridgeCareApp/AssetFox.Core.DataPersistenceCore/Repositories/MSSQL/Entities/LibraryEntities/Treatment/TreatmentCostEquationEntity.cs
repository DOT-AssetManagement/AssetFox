using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Treatment
{
    public class TreatmentCostEquationEntity : BaseEquationJoinEntity
    {
        public Guid TreatmentCostId { get; set; }
        public virtual TreatmentCostEntity TreatmentCost { get; set; }
    }
}
