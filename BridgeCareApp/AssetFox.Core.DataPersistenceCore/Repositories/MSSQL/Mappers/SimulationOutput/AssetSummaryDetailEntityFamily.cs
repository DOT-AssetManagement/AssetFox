using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL
{
    public class AssetSummaryDetailEntityFamily
    {
        public List<AssetSummaryDetailValueEntityIntId> AssetSummaryDetailValues { get; set; } = new List<AssetSummaryDetailValueEntityIntId>();
        public List<AssetSummaryDetailEntity> AssetSummaryDetails { get; set; } = new List<AssetSummaryDetailEntity>();
    }
}
