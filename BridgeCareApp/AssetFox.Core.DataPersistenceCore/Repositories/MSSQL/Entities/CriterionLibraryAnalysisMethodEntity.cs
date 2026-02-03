using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class CriterionLibraryAnalysisMethodEntity : BaseEntity
    {
        public Guid CriterionLibraryId { get; set; }

        public Guid AnalysisMethodId { get; set; }

        public virtual AnalysisMethodEntity AnalysisMethod { get; set; }

        public virtual CriterionLibraryEntity CriterionLibrary { get; set; }
    }
}
