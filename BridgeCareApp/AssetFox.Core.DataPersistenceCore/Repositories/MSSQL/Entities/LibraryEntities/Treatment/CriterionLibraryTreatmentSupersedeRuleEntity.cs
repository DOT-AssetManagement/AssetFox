using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Treatment
{
    public class CriterionLibraryTreatmentSupersedeRuleEntity : BaseCriterionLibraryJoinEntity
    {
        public Guid TreatmentSupersedeRuleId { get; set; }        

        public virtual TreatmentSupersedeRuleEntity TreatmentSupersedeRule { get; set; }
    }
}
