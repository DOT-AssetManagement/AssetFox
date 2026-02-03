using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.RemainingLifeLimit
{
    public class CriterionLibraryRemainingLifeLimitEntity : BaseCriterionLibraryJoinEntity
    {
        public Guid RemainingLifeLimitId { get; set; }

        public virtual RemainingLifeLimitEntity RemainingLifeLimit { get; set; }
    }
}
