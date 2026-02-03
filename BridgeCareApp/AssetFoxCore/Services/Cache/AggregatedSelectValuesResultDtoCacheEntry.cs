using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services
{
    public class AggregatedSelectValuesResultDtoCacheEntry
    {
        public AggregatedSelectValuesResultDTO Dto { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}
