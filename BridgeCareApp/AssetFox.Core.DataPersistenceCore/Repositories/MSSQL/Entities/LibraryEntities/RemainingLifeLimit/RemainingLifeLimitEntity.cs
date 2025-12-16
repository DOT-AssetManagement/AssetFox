using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.RemainingLifeLimit
{
    public class RemainingLifeLimitEntity : BaseRemainingLifeLimitEntity
    {
        public Guid RemainingLifeLimitLibraryId { get; set; }

        public double Value { get; set; }

        public virtual RemainingLifeLimitLibraryEntity RemainingLifeLimitLibrary { get; set; }

        public virtual CriterionLibraryRemainingLifeLimitEntity CriterionLibraryRemainingLifeLimitJoin { get; set; }
    }
}
