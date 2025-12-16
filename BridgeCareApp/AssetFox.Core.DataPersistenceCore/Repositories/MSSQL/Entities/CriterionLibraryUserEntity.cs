using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class CriterionLibraryUserEntity
    {
        public Guid CriterionLibraryId { get; set; }

        public Guid UserId { get; set; }

        public virtual CriterionLibraryEntity CriterionLibrary { get; set; }

        public virtual UserEntity User { get; set; }
    }
}
