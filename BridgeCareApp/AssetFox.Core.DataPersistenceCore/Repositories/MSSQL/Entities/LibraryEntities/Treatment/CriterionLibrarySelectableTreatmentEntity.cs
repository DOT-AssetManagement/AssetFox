using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Treatment
{
    public class CriterionLibrarySelectableTreatmentEntity : BaseEntity
    {
        public Guid CriterionLibraryId { get; set; }

        public Guid SelectableTreatmentId { get; set; }

        public virtual CriterionLibraryEntity CriterionLibrary { get; set; }

        public virtual SelectableTreatmentEntity SelectableTreatment { get; set; }
    }
}
