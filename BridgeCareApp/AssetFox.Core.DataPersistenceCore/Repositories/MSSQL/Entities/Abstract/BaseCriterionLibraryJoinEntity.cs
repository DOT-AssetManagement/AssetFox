using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract
{
    public abstract class BaseCriterionLibraryJoinEntity : BaseEntity
    {
        public Guid CriterionLibraryId { get; set; }

        public virtual CriterionLibraryEntity CriterionLibrary { get; set; }
    }
}
