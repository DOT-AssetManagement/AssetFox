using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class AssetDetailValueEntityIntId: SimulationOutputValueEntityIntId
    {
        public Guid AssetDetailId { get; set; }

        public int RunId { get; set; }

        public virtual AssetDetailEntity AssetDetail { get; set; }

    }
}
