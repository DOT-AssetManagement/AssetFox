using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Treatment
{
    public class ConditionalTreatmentConsequenceEntity : TreatmentConsequenceEntity
    {
        public Guid SelectableTreatmentId { get; set; }

        public virtual SelectableTreatmentEntity SelectableTreatment { get; set; }

        public virtual CriterionLibraryConditionalTreatmentConsequenceEntity CriterionLibraryConditionalTreatmentConsequenceJoin { get; set; }

        public virtual ConditionalTreatmentConsequenceEquationEntity ConditionalTreatmentConsequenceEquationJoin { get; set; }
    }
}
