using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public abstract class SimulationOutputValueEntityIntId
    {
        public long Id { get; set; }

        [Column(TypeName = "char(1)")]
        public char Discriminator { get; set; }

        public string TextValue { get; set; }

        public double? NumericValue { get; set; }

        public Guid AttributeId { get; set; }

        public virtual AttributeEntity Attribute { get; set; }
    }
}
