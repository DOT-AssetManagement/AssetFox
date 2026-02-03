using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class BenefitQuantifierEntity : BaseEntity
    {
        public Guid NetworkId { get; set; }

        public Guid EquationId { get; set; }

        public virtual NetworkEntity Network { get; set; }

        public virtual EquationEntity Equation { get; set; }
    }
}
